using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.Hl25.MiniApp;
using Genora.MultiTenancy.DomainModels.AppHl25;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Hl25;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Uow;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hl25;

/// <summary>
/// AppService phục vụ Zalo Mini App "Dược Phẩm Hoa Linh 25 Năm" (public/anonymous).
/// Định danh người chơi qua ZaloUserId (upsert theo tenant hiện tại).
/// </summary>
[AllowAnonymous]
[RemoteService(false)]
[DisableValidation]
public class MiniAppHl25Service : ApplicationService, IMiniAppHl25Service
{
    private readonly IRepository<Hl25AppConfig, Guid> _configRepository;
    private readonly IRepository<Hl25Participant, Guid> _participantRepository;
    private readonly IRepository<Hl25FrameCampaign, Guid> _frameCampaignRepository;
    private readonly IRepository<Hl25FrameTemplate, Guid> _frameTemplateRepository;
    private readonly IRepository<Hl25FrameCreation, Guid> _frameCreationRepository;
    private readonly IRepository<Hl25SpinTurnLog, Guid> _spinTurnLogRepository;
    private readonly IRepository<Hl25WheelConfig, Guid> _wheelConfigRepository;
    private readonly IRepository<Hl25WheelSlot, Guid> _wheelSlotRepository;
    private readonly IRepository<Hl25Gift, Guid> _giftRepository;
    private readonly IRepository<Hl25SpinLog, Guid> _spinLogRepository;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly IManageImageService _manageImageService;
    private readonly IHttpContextAccessor _httpContextAccessor;

    public MiniAppHl25Service(
        IRepository<Hl25AppConfig, Guid> configRepository,
        IRepository<Hl25Participant, Guid> participantRepository,
        IRepository<Hl25FrameCampaign, Guid> frameCampaignRepository,
        IRepository<Hl25FrameTemplate, Guid> frameTemplateRepository,
        IRepository<Hl25FrameCreation, Guid> frameCreationRepository,
        IRepository<Hl25SpinTurnLog, Guid> spinTurnLogRepository,
        IRepository<Hl25WheelConfig, Guid> wheelConfigRepository,
        IRepository<Hl25WheelSlot, Guid> wheelSlotRepository,
        IRepository<Hl25Gift, Guid> giftRepository,
        IRepository<Hl25SpinLog, Guid> spinLogRepository,
        IUnitOfWorkManager uowManager,
        IManageImageService manageImageService,
        IHttpContextAccessor httpContextAccessor)
    {
        _configRepository = configRepository;
        _participantRepository = participantRepository;
        _frameCampaignRepository = frameCampaignRepository;
        _frameTemplateRepository = frameTemplateRepository;
        _frameCreationRepository = frameCreationRepository;
        _spinTurnLogRepository = spinTurnLogRepository;
        _wheelConfigRepository = wheelConfigRepository;
        _wheelSlotRepository = wheelSlotRepository;
        _giftRepository = giftRepository;
        _spinLogRepository = spinLogRepository;
        _uowManager = uowManager;
        _manageImageService = manageImageService;
        _httpContextAccessor = httpContextAccessor;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    // ===== Cấu hình =====
    public async Task<Hl25MiniAppConfigDto> GetConfigAsync()
    {
        var queryable = await _configRepository.GetQueryableAsync();
        var config = await AsyncExecuter.FirstOrDefaultAsync(queryable);

        if (config == null)
            return new Hl25MiniAppConfigDto { IsActive = false };

        return new Hl25MiniAppConfigDto
        {
            ProgramName = config.ProgramName,
            IntroductionHtml = config.IntroductionHtml,
            Format = config.Format,
            GiftDeliveryTime = config.GiftDeliveryTime,
            RulesHtml = config.RulesHtml,
            StartTime = config.StartTime,
            EndTime = config.EndTime,
            Scope = config.Scope,
            OrganizerName = config.OrganizerName,
            IsActive = config.IsActive
        };
    }

    // ===== Người tham gia =====
    public async Task<Hl25MeDto> RegisterAsync(Hl25RegisterRequest request)
    {
        ValidateZaloUserId(request.ZaloUserId);

        var participant = await FindByZaloUserIdAsync(request.ZaloUserId);

        if (participant == null)
        {
            participant = new Hl25Participant(GuidGenerator.Create(), CurrentTenant.Id)
            {
                ZaloUserId = request.ZaloUserId,
                FullName = request.FullName,
                PhoneNumber = request.PhoneNumber,
                AvatarUrl = request.AvatarUrl,
                IsFollowingOa = request.IsFollowingOa ?? false,
                HasConsent = request.HasConsent ?? false
            };
            if (participant.HasConsent)
                participant.ConsentTime = DateTime.Now;

            participant = await _participantRepository.InsertAsync(participant, autoSave: true);
        }
        else
        {
            // Cập nhật thông tin nếu có gửi lên (không ghi đè bằng null).
            participant.FullName = request.FullName ?? participant.FullName;
            participant.PhoneNumber = request.PhoneNumber ?? participant.PhoneNumber;
            participant.AvatarUrl = request.AvatarUrl ?? participant.AvatarUrl;
            if (request.IsFollowingOa.HasValue)
                participant.IsFollowingOa = request.IsFollowingOa.Value;
            if (request.HasConsent == true && !participant.HasConsent)
            {
                participant.HasConsent = true;
                participant.ConsentTime = DateTime.Now;
            }
            participant = await _participantRepository.UpdateAsync(participant, autoSave: true);
        }

        return MapMe(participant);
    }

    public async Task<Hl25MeDto> GetMeAsync(string zaloUserId)
    {
        ValidateZaloUserId(zaloUserId);
        var participant = await FindByZaloUserIdAsync(zaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");
        return MapMe(participant);
    }

    public async Task<Hl25MeDto> UpdateProfileAsync(Hl25UpdateProfileRequest request)
    {
        ValidateZaloUserId(request.ZaloUserId);
        var participant = await FindByZaloUserIdAsync(request.ZaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        participant.FullName = request.FullName;
        participant.PhoneNumber = request.PhoneNumber;
        participant.AgeGroup = request.AgeGroup;
        participant.Gender = request.Gender;
        participant.ReceiveAddress = request.ReceiveAddress;

        participant = await _participantRepository.UpdateAsync(participant, autoSave: true);
        return MapMe(participant);
    }

    // ===== Tạo thiệp =====
    public async Task<Hl25FrameResultDto> CreateFrameAsync(Hl25CreateFrameRequest request)
    {
        ValidateZaloUserId(request.ZaloUserId);
        if (string.IsNullOrWhiteSpace(request.ResultImageUrl))
            throw new UserFriendlyException(Hl25ErrorCodes.FrameImageRequired, "Thiếu ảnh thiệp.");

        if (request.WishMessage != null && request.WishMessage.Length > Hl25Consts.MaxWishLength)
            throw new UserFriendlyException(Hl25ErrorCodes.WishTooLong, $"Lời chúc tối đa {Hl25Consts.MaxWishLength} ký tự.");

        var participant = await FindByZaloUserIdAsync(request.ZaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        var creation = new Hl25FrameCreation(GuidGenerator.Create(), participant.Id, request.ResultImageUrl, CurrentTenant.Id)
        {
            CampaignId = request.CampaignId,
            TemplateId = request.TemplateId,
            WishMessage = request.WishMessage
        };
        creation = await _frameCreationRepository.InsertAsync(creation, autoSave: true);

        return new Hl25FrameResultDto
        {
            FrameCreationId = creation.Id,
            ResultImageUrl = creation.ResultImageUrl,
            ShareLink = creation.ShareLink
        };
    }

    // ===== Chia sẻ → cộng lượt theo chu kỳ =====
    public async Task<Hl25ShareResultDto> ShareFrameAsync(Hl25ShareFrameRequest request)
    {
        ValidateZaloUserId(request.ZaloUserId);

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        var participant = await FindByZaloUserIdAsync(request.ZaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        var creation = await _frameCreationRepository.FindAsync(request.FrameCreationId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.FrameNotFound, "Không tìm thấy thiệp.");

        if (creation.ParticipantId != participant.Id)
            throw new UserFriendlyException(Hl25ErrorCodes.FrameNotOwned, "Thiệp không thuộc về người dùng này.");

        // Chu kỳ đã hoàn tất trước đó (đã chia sẻ) → không cộng lượt lần nữa.
        if (creation.SharePlatform != Hl25SharePlatform.None)
        {
            await uow.CompleteAsync();
            return new Hl25ShareResultDto
            {
                TurnGranted = false,
                RemainingSpinTurns = participant.RemainingSpinTurns,
                EarnedCycles = participant.EarnedCycles,
                Message = "Thiệp này đã được chia sẻ trước đó."
            };
        }

        // Đánh dấu đã chia sẻ.
        creation.SharePlatform = request.SharePlatform;
        creation.ShareTime = DateTime.Now;
        await _frameCreationRepository.UpdateAsync(creation, autoSave: false);

        bool granted = false;
        // Cộng lượt nếu chưa đạt trần chu kỳ (mỗi chu kỳ Tạo thiệp→Chia sẻ = +1 lượt).
        if (participant.EarnedCycles < Hl25Consts.MaxSpinTurnsPerUser)
        {
            participant.EarnedCycles += 1;
            participant.RemainingSpinTurns += 1;
            participant.TotalSpinTurns += 1;
            await _participantRepository.UpdateAsync(participant, autoSave: false);

            var source = request.SharePlatform == Hl25SharePlatform.Facebook
                ? Hl25SpinTurnSource.ShareFacebook
                : Hl25SpinTurnSource.ShareZalo;
            var turnLog = new Hl25SpinTurnLog(GuidGenerator.Create(), participant.Id, source, 1, CurrentTenant.Id)
            {
                FrameCreationId = creation.Id,
                Note = "Cộng lượt do chia sẻ thiệp"
            };
            await _spinTurnLogRepository.InsertAsync(turnLog, autoSave: false);
            granted = true;
        }

        await uow.CompleteAsync();

        return new Hl25ShareResultDto
        {
            TurnGranted = granted,
            RemainingSpinTurns = participant.RemainingSpinTurns,
            EarnedCycles = participant.EarnedCycles,
            Message = granted
                ? "Bạn nhận thêm 1 lượt quay!"
                : $"Bạn đã đạt tối đa {Hl25Consts.MaxSpinTurnsPerUser} lượt quay."
        };
    }

    // ===== Vòng quay =====
    public async Task<Hl25MiniAppWheelDto> GetWheelAsync(string zaloUserId)
    {
        ValidateZaloUserId(zaloUserId);

        var wheelQueryable = await _wheelConfigRepository.GetQueryableAsync();
        var config = await AsyncExecuter.FirstOrDefaultAsync(wheelQueryable);

        var participant = await FindByZaloUserIdAsync(zaloUserId);
        var remaining = participant?.RemainingSpinTurns ?? 0;

        if (config == null)
            return new Hl25MiniAppWheelDto { IsActive = false, RemainingSpinTurns = remaining };

        var slotQueryable = await _wheelSlotRepository.GetQueryableAsync();
        var slots = await AsyncExecuter.ToListAsync(
            slotQueryable.Where(x => x.WheelConfigId == config.Id).OrderBy(x => x.DisplayOrder));

        // Nạp thông tin quà đã gán để mapping ảnh/tên/mô tả cho từng ô.
        var giftIds = slots.Where(x => x.GiftId.HasValue).Select(x => x.GiftId!.Value).Distinct().ToList();
        var giftMap = new Dictionary<Guid, Hl25Gift>();
        if (giftIds.Count > 0)
        {
            var giftQueryable = await _giftRepository.GetQueryableAsync();
            var gifts = await AsyncExecuter.ToListAsync(giftQueryable.Where(g => giftIds.Contains(g.Id)));
            giftMap = gifts.ToDictionary(g => g.Id);
        }

        return new Hl25MiniAppWheelDto
        {
            Title = config.Title,
            SubTitle = config.SubTitle,
            PrimaryColor = config.PrimaryColor,
            SecondaryColor = config.SecondaryColor,
            BackgroundImageUrl = ToFullUrl(config.BackgroundImageUrl),
            PointerImageUrl = ToFullUrl(config.PointerImageUrl),
            IsActive = config.IsActive,
            RemainingSpinTurns = remaining,
            // KHÔNG trả WinRate (bảo mật tỷ lệ trúng).
            Slots = slots.Select(s =>
            {
                Hl25Gift? gift = s.GiftId.HasValue && giftMap.TryGetValue(s.GiftId.Value, out var g) ? g : null;
                return new Hl25MiniAppWheelSlotDto
                {
                    Id = s.Id,
                    // Ưu tiên ảnh riêng của ô; nếu trống thì lấy ảnh quà đã gán.
                    SlotImageUrl = ToFullUrl(!string.IsNullOrWhiteSpace(s.SlotImageUrl) ? s.SlotImageUrl : gift?.ImageUrl),
                    // Ưu tiên nhãn riêng của ô; nếu trống thì lấy tên quà đã gán.
                    Label = !string.IsNullOrWhiteSpace(s.Label) ? s.Label : gift?.Name,
                    DisplayOrder = s.DisplayOrder,
                    ColorHex = s.ColorHex,
                    GiftId = s.GiftId,
                    GiftName = gift?.Name,
                    GiftDescription = gift?.Description,
                    IsGift = gift != null
                };
            }).ToList()
        };
    }

    // ===== Thực hiện quay (ACID) =====
    public async Task<Hl25SpinResultDto> SpinAsync(Hl25SpinRequest request)
    {
        ValidateZaloUserId(request.ZaloUserId);

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        var participant = await FindByZaloUserIdAsync(request.ZaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        // Kiểm tra lượt còn lại TRONG transaction (validate-then-write).
        if (participant.RemainingSpinTurns <= 0)
            throw new UserFriendlyException(Hl25ErrorCodes.NoSpinTurns, "Bạn đã hết lượt quay.");

        var wheelQueryable = await _wheelConfigRepository.GetQueryableAsync();
        var config = await AsyncExecuter.FirstOrDefaultAsync(wheelQueryable)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.WheelNotConfigured, "Vòng quay chưa được cấu hình.");
        if (!config.IsActive)
            throw new UserFriendlyException(Hl25ErrorCodes.WheelInactive, "Vòng quay đang tạm dừng.");

        var slotQueryable = await _wheelSlotRepository.GetQueryableAsync();
        var slots = await AsyncExecuter.ToListAsync(
            slotQueryable.Where(x => x.WheelConfigId == config.Id).OrderBy(x => x.DisplayOrder));
        if (slots.Count == 0)
            throw new UserFriendlyException(Hl25ErrorCodes.WheelNoSlots, "Vòng quay chưa có ô quay.");

        // Delta 2026-09 — mỗi người TỐI ĐA TRÚNG 1 LẦN trong toàn chương trình.
        // Nếu đã trúng trước đó (TotalGiftsWon >= 1) thì lượt này ép KHÔNG trúng
        // (lần 1 trúng → lần 2 ép trượt; lần 1 trượt → lần 2 quay ngẫu nhiên bình thường).
        var hasWonBefore = participant.TotalGiftsWon >= 1;

        // Chọn ô theo tỷ lệ WinRate (weighted random).
        var selected = PickSlotByWinRate(slots);

        // Trừ 1 lượt quay.
        participant.RemainingSpinTurns -= 1;

        var spinLog = new Hl25SpinLog(GuidGenerator.Create(), participant.Id, CurrentTenant.Id)
        {
            WheelSlotId = selected.Id,
            ReceiverAddressSnapshot = participant.ReceiveAddress
        };

        bool won = false;
        Hl25Gift? gift = null;

        // Chỉ xét trúng khi CHƯA từng trúng lần nào (trần trúng 1 lần/người).
        if (!hasWonBefore && selected.GiftId.HasValue)
        {
            gift = await _giftRepository.FindAsync(selected.GiftId.Value);
            // Trúng chỉ khi quà còn hàng + đang bật.
            if (gift != null && gift.Status == Hl25GiftStatus.Available && gift.RemainingQuantity > 0)
            {
                gift.RemainingQuantity -= 1;
                if (gift.RemainingQuantity <= 0)
                    gift.Status = Hl25GiftStatus.OutOfStock;
                await _giftRepository.UpdateAsync(gift, autoSave: false);

                spinLog.GiftId = gift.Id;
                spinLog.GiftNameSnapshot = gift.Name;
                spinLog.RewardStatus = Hl25RewardStatus.Won;
                participant.TotalGiftsWon += 1;
                won = true;
            }
            else
            {
                // Ô có quà nhưng hết kho → coi như không trúng.
                gift = null;
                spinLog.RewardStatus = Hl25RewardStatus.NotWon;
            }
        }
        else
        {
            // Ô "Chúc may mắn" HOẶC đã trúng trước đó (ép trượt).
            spinLog.RewardStatus = Hl25RewardStatus.NotWon;
        }

        await _participantRepository.UpdateAsync(participant, autoSave: false);
        await _spinLogRepository.InsertAsync(spinLog, autoSave: false);

        await uow.CompleteAsync();

        return new Hl25SpinResultDto
        {
            Won = won,
            SpinLogId = spinLog.Id,
            SlotId = selected.Id,
            GiftId = won ? gift!.Id : null,
            GiftName = won ? gift!.Name : null,
            GiftImageUrl = won ? gift!.ImageUrl : null,
            RemainingSpinTurns = participant.RemainingSpinTurns,
            // Cờ FE (Delta 2026-09) cho 3 màn kết quả.
            CanShareForMoreTurn = participant.EarnedCycles < Hl25Consts.MaxSpinTurnsPerUser,
            EarnedCycles = participant.EarnedCycles,
            TotalGiftsWon = participant.TotalGiftsWon,
            HasWonBefore = hasWonBefore
        };
    }

    // ===== Lịch sử nhận quà =====
    public async Task<List<Hl25MyGiftDto>> GetMyGiftsAsync(string zaloUserId)
    {
        ValidateZaloUserId(zaloUserId);
        var participant = await FindByZaloUserIdAsync(zaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        var spinQueryable = await _spinLogRepository.GetQueryableAsync();
        var giftQueryable = await _giftRepository.GetQueryableAsync();

        var query = from s in spinQueryable
                    where s.ParticipantId == participant.Id && s.GiftId != null
                    join g in giftQueryable on s.GiftId equals g.Id into gg
                    from g in gg.DefaultIfEmpty()
                    orderby s.SpinTime descending
                    select new { s, g };

        var rows = await AsyncExecuter.ToListAsync(query);

        return rows.Select(x => new Hl25MyGiftDto
        {
            SpinLogId = x.s.Id,
            GiftId = x.s.GiftId,
            GiftName = x.s.GiftNameSnapshot ?? x.g?.Name,
            GiftImageUrl = x.g?.ImageUrl,
            SpinTime = x.s.SpinTime,
            RewardStatus = x.s.RewardStatus,
            DeliveredTime = x.s.DeliveredTime
        }).ToList();
    }

    // ===== (Delta 2026-09) Frame — public read =====
    public async Task<List<Hl25FrameCampaignPublicDto>> GetFrameCampaignsAsync()
    {
        var campaignQueryable = await _frameCampaignRepository.GetQueryableAsync();
        var templateQueryable = await _frameTemplateRepository.GetQueryableAsync();

        // Chỉ lấy chiến dịch đang hoạt động (Active).
        var campaigns = await AsyncExecuter.ToListAsync(
            campaignQueryable.Where(x => x.Status == Hl25CampaignStatus.Active)
                             .OrderByDescending(x => x.StartTime));

        // Đếm số mẫu frame đang bật theo từng chiến dịch.
        var activeTemplates = await AsyncExecuter.ToListAsync(
            templateQueryable.Where(x => x.IsActive));
        var countByCampaign = activeTemplates
            .GroupBy(x => x.CampaignId)
            .ToDictionary(g => g.Key, g => g.Count());

        return campaigns.Select(c => new Hl25FrameCampaignPublicDto
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            StartTime = c.StartTime,
            EndTime = c.EndTime,
            Status = c.Status,
            TemplateCount = countByCampaign.TryGetValue(c.Id, out var n) ? n : 0
        }).ToList();
    }

    public async Task<List<Hl25FrameTemplatePublicDto>> GetFrameTemplatesAsync(Guid? campaignId)
    {
        var queryable = await _frameTemplateRepository.GetQueryableAsync();
        var query = queryable.Where(x => x.IsActive);

        if (campaignId.HasValue)
            query = query.Where(x => x.CampaignId == campaignId.Value);

        var templates = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.DisplayOrder));

        return templates.Select(t => new Hl25FrameTemplatePublicDto
        {
            Id = t.Id,
            CampaignId = t.CampaignId,
            Name = t.Name,
            ImageUrl = ToFullUrl(t.ImageUrl)!,
            ThumbnailUrl = ToFullUrl(t.ThumbnailUrl),
            DisplayOrder = t.DisplayOrder
        }).ToList();
    }

    public async Task<List<Hl25FrameCreationPublicDto>> GetMyFrameCreationsAsync(string zaloUserId)
    {
        ValidateZaloUserId(zaloUserId);
        var participant = await FindByZaloUserIdAsync(zaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        var queryable = await _frameCreationRepository.GetQueryableAsync();
        var creations = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.ParticipantId == participant.Id)
                     .OrderByDescending(x => x.CreatedTime));

        return creations.Select(c => new Hl25FrameCreationPublicDto
        {
            Id = c.Id,
            CampaignId = c.CampaignId,
            TemplateId = c.TemplateId,
            ResultImageUrl = ToFullUrl(c.ResultImageUrl)!,
            WishMessage = c.WishMessage,
            ShareLink = c.ShareLink,
            SharePlatform = c.SharePlatform,
            ShareTime = c.ShareTime,
            CreatedTime = c.CreatedTime
        }).ToList();
    }

    // ===== (Delta 2026-09) Wheel — public read =====
    public async Task<List<Hl25GiftPublicDto>> GetGiftsAsync()
    {
        var queryable = await _giftRepository.GetQueryableAsync();
        // Ẩn quà bị vô hiệu hóa (Disabled) — chỉ trả Available + OutOfStock để FE hiển thị cơ cấu giải.
        var gifts = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.Status != Hl25GiftStatus.Disabled)
                     .OrderBy(x => x.Name));

        return gifts.Select(g => new Hl25GiftPublicDto
        {
            Id = g.Id,
            Name = g.Name,
            ImageUrl = ToFullUrl(g.ImageUrl),
            Description = g.Description,
            Value = g.Value,
            Status = g.Status
        }).ToList();
    }

    public async Task<List<Hl25SpinTurnLogPublicDto>> GetMySpinTurnLogsAsync(string zaloUserId)
    {
        ValidateZaloUserId(zaloUserId);
        var participant = await FindByZaloUserIdAsync(zaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        var queryable = await _spinTurnLogRepository.GetQueryableAsync();
        var logs = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.ParticipantId == participant.Id)
                     .OrderByDescending(x => x.GrantedTime));

        return logs.Select(l => new Hl25SpinTurnLogPublicDto
        {
            Id = l.Id,
            Source = l.Source,
            TurnsAdded = l.TurnsAdded,
            Note = l.Note,
            GrantedTime = l.GrantedTime
        }).ToList();
    }

    public async Task<List<Hl25SpinLogPublicDto>> GetMySpinLogsAsync(string zaloUserId)
    {
        ValidateZaloUserId(zaloUserId);
        var participant = await FindByZaloUserIdAsync(zaloUserId)
            ?? throw new UserFriendlyException(Hl25ErrorCodes.ParticipantNotFound, "Người dùng chưa đăng ký chương trình.");

        var spinQueryable = await _spinLogRepository.GetQueryableAsync();
        var giftQueryable = await _giftRepository.GetQueryableAsync();

        var query = from s in spinQueryable
                    where s.ParticipantId == participant.Id
                    join g in giftQueryable on s.GiftId equals g.Id into gg
                    from g in gg.DefaultIfEmpty()
                    orderby s.SpinTime descending
                    select new { s, g };

        var rows = await AsyncExecuter.ToListAsync(query);

        return rows.Select(x => new Hl25SpinLogPublicDto
        {
            Id = x.s.Id,
            GiftId = x.s.GiftId,
            GiftName = x.s.GiftNameSnapshot ?? x.g?.Name,
            GiftImageUrl = ToFullUrl(x.g?.ImageUrl),
            SpinTime = x.s.SpinTime,
            RewardStatus = x.s.RewardStatus,
            DeliveredTime = x.s.DeliveredTime,
            Won = x.s.RewardStatus == Hl25RewardStatus.Won || x.s.RewardStatus == Hl25RewardStatus.Delivered
        }).ToList();
    }

    // ===== (Delta 2026-09) Upload ảnh =====
    public async Task<Hl25UploadImageResultDto> UploadImageAsync(IRemoteStreamContent file)
    {
        if (file == null || (file.ContentLength ?? 0) == 0)
            throw new UserFriendlyException(Hl25ErrorCodes.ImageRequired, "Thiếu file ảnh.");

        // Tự validate 5MB (ManageImageService KHÔNG chặn size).
        var length = file.ContentLength ?? file.GetStream().Length;
        if (length > Hl25Consts.MaxCardImageSizeBytes)
            throw new UserFriendlyException(Hl25ErrorCodes.ImageTooLarge,
                $"Ảnh vượt quá dung lượng cho phép ({Hl25Consts.MaxCardImageSizeBytes / (1024 * 1024)}MB).");

        try
        {
            var relativePath = await _manageImageService.UploadImageAsync(
                file, CurrentTenant.Id?.ToString() ?? "host", Hl25Consts.DefaultImageSubFolder);

            return new Hl25UploadImageResultDto { Url = ToFullUrl(relativePath)! };
        }
        catch (UserFriendlyException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new UserFriendlyException(Hl25ErrorCodes.UploadFailed, "Upload ảnh thất bại.")
                .WithData("Detail", ex.Message);
        }
    }

    // ===== Helpers =====
    private async Task<Hl25Participant?> FindByZaloUserIdAsync(string zaloUserId)
    {
        var queryable = await _participantRepository.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(x => x.ZaloUserId == zaloUserId));
    }

    /// <summary>
    /// Dựng URL đầy đủ (scheme + host + path) từ path tương đối lưu trong DB (VD "/uploads/hl25/host/abc.png").
    /// Idempotent: nếu đã là URL tuyệt đối (http/https) thì giữ nguyên (VD avatar Zalo). Null → null.
    /// </summary>
    private string? ToFullUrl(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return path;

        if (path.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
            path.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            return path;

        var request = _httpContextAccessor.HttpContext?.Request;
        if (request == null)
            return path; // không có HTTP context (VD test) → trả nguyên path tương đối.

        var baseUrl = $"{request.Scheme}://{request.Host.Value}";
        return path.StartsWith("/") ? baseUrl + path : baseUrl + "/" + path;
    }

    private static void ValidateZaloUserId(string zaloUserId)
    {
        if (string.IsNullOrWhiteSpace(zaloUserId))
            throw new UserFriendlyException(Hl25ErrorCodes.MissingZaloUserId, "Thiếu ZaloUserId.");
    }

    /// <summary>
    /// Chọn ô theo tỷ lệ trúng WinRate (weighted random). Nếu tổng WinRate = 0 → chọn ngẫu nhiên đều.
    /// </summary>
    private static Hl25WheelSlot PickSlotByWinRate(List<Hl25WheelSlot> slots)
    {
        var totalRate = slots.Sum(x => x.WinRate);
        if (totalRate <= 0)
            return slots[Random.Shared.Next(slots.Count)];

        var roll = (decimal)Random.Shared.NextDouble() * totalRate;
        decimal cumulative = 0;
        foreach (var slot in slots)
        {
            cumulative += slot.WinRate;
            if (roll < cumulative)
                return slot;
        }
        return slots[^1];
    }

    private static Hl25MeDto MapMe(Hl25Participant p) => new()
    {
        Id = p.Id,
        ZaloUserId = p.ZaloUserId,
        FullName = p.FullName,
        PhoneNumber = p.PhoneNumber,
        AgeGroup = p.AgeGroup,
        Gender = p.Gender,
        ReceiveAddress = p.ReceiveAddress,
        AvatarUrl = p.AvatarUrl,
        IsFollowingOa = p.IsFollowingOa,
        HasConsent = p.HasConsent,
        RemainingSpinTurns = p.RemainingSpinTurns,
        TotalSpinTurns = p.TotalSpinTurns,
        EarnedCycles = p.EarnedCycles,
        TotalGiftsWon = p.TotalGiftsWon
    };
}
