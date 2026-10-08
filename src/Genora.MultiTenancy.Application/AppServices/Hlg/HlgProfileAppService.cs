using System;
using System.Collections.Generic;
using System.Linq;
using Genora.MultiTenancy.AppHlg;
using Genora.MultiTenancy.AppServices.HoaLinh;
using Genora.MultiTenancy.Hlg;
using Volo.Abp.Uow;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppHlPoints;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Data;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hlg;

/// <summary>
/// Hồ sơ người chơi Gamification. Tái dùng dbo.AppCustomers (zalo/phone/code/BonusPoint)
/// + HLG.AppHlgUserProfiles cho field game (customerType, isRegistered).
/// Point history tái dùng ledger HL.AppHlPointTransactions (BonusPoint dùng chung).
/// Internal service — controller gọi trực tiếp.
/// </summary>
[RemoteService(false)]
[DisableValidation]
public partial class HlgProfileAppService : ApplicationService, IHlgProfileAppService
{
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly IRepository<HlgUserProfile, Guid> _profileRepo;
    private readonly IRepository<HlPointTransaction, Guid> _pointTxnRepo;
    private readonly IRepository<HlgLearningProgress, Guid> _progressRepo;
    private readonly IRepository<HlgGameSession, Guid> _sessionRepo;
    private readonly IRepository<HlgProduct, Guid> _productRepo;
    private readonly IRepository<HlgRewardHistory, Guid> _rewardHistoryRepo;
    private readonly IHlgGameAppService _gameService;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<HlgProfileAppService> _logger;
    private readonly IConfiguration _configuration;
    private readonly IHlApiClientService _dms;
    private readonly IHlgRegistrationLock _registrationLock;
    private readonly IUnitOfWorkManager _registrationUow;

    public HlgProfileAppService(
        IRepository<Customer, Guid> customerRepo,
        IRepository<HlgUserProfile, Guid> profileRepo,
        IRepository<HlPointTransaction, Guid> pointTxnRepo,
        IRepository<HlgLearningProgress, Guid> progressRepo,
        IRepository<HlgGameSession, Guid> sessionRepo,
        IRepository<HlgProduct, Guid> productRepo,
        IRepository<HlgRewardHistory, Guid> rewardHistoryRepo,
        IHlgGameAppService gameService,
        ICurrentTenant currentTenant,
        ILogger<HlgProfileAppService> logger,
        IConfiguration configuration,
        IHlApiClientService dms,
        IHlgRegistrationLock registrationLock,
        IUnitOfWorkManager registrationUow)
    {
        LocalizationResource = typeof(Genora.MultiTenancy.Localization.MultiTenancyResource);
        _customerRepo = customerRepo;
        _profileRepo = profileRepo;
        _pointTxnRepo = pointTxnRepo;
        _progressRepo = progressRepo;
        _sessionRepo = sessionRepo;
        _productRepo = productRepo;
        _rewardHistoryRepo = rewardHistoryRepo;
        _gameService = gameService;
        _currentTenant = currentTenant;
        _logger = logger;
        _configuration = configuration;
        _dms = dms;
        _registrationLock = registrationLock;
        _registrationUow = registrationUow;
    }

    public async Task<GamificationUserDto> GetByPhoneAsync(string phone, Guid? gameId = null, CancellationToken ct = default)
    {
        var (customer, profile) = await ResolveAsync(phone, ct);
        var dto = MapToDto(customer, profile);
        // Màn trước khi bấm "Chơi ngay": nếu FE truyền gameId và khách đã HOÀN THÀNH game đó,
        // trả cờ để FE hiển thị modal (xem lịch sử / bảng xếp hạng) ngay, thay vì vào màn chơi rồi mới báo.
        if (gameId.HasValue && gameId.Value != Guid.Empty
            && await _gameService.HasPassedGameAsync(gameId.Value, customer.Id, ct))
        {
            dto.AlreadyCompleted = true;
            dto.AlreadyCompletedMessage = HlgGameAppService.AlreadyCompletedMessage;
        }
        return dto;
    }

    public async Task<GamificationUserDto> UpdateProfileAsync(string phone, UpdateProfilePayloadDto payload, CancellationToken ct = default)
    {
        var (customer, profile) = await ResolveAsync(phone, ct);
        if (!profile.IsRegistered)
            throw new UserFriendlyException("Vui lòng hoàn tất đăng ký qua customer/upsert trước.");
        if (profile.PharmaPhone != null && !string.IsNullOrWhiteSpace(payload.Phone)
            && NormalizePhone(payload.Phone) != NormalizePhone(customer.PhoneNumber))
            throw new UserFriendlyException("Không thể đổi số điện thoại của tài khoản đã liên kết nhà thuốc.");

        if (!string.IsNullOrWhiteSpace(payload.FullName))
            customer.FullName = payload.FullName.Trim();

        var genderByte = HlgEnumMapper.GenderStringToByte(payload.Gender);
        if (genderByte.HasValue) customer.Gender = genderByte;

        var bday = ParseDate(payload.Birthday);
        if (bday.HasValue) customer.DateOfBirth = bday;

        if (!string.IsNullOrWhiteSpace(payload.Address))
            customer.Address = payload.Address.Trim();

        ValidatePharmacyCode(payload.PharmacyCode ?? payload.VgaCode);
        ValidateCustomerType(payload.CustomerType);
        if (payload.PharmacyCode != null || payload.VgaCode != null) profile.PharmacyCode = NullIfBlank(payload.PharmacyCode ?? payload.VgaCode);
        if (payload.CustomerType != null) profile.CustomerType = HlgEnumMapper.CustomerTypeFromString(payload.CustomerType);
        await _profileRepo.UpdateAsync(profile, autoSave: true, cancellationToken: ct);

        // Phone là khóa đồng bộ; chỉ đổi khi khác và chưa bị chiếm bởi KH khác.
        var newPhone = NormalizePhone(payload.Phone);
        if (!string.IsNullOrWhiteSpace(newPhone) && newPhone != customer.PhoneNumber)
        {
            var taken = await _customerRepo.AnyAsync(x => x.TenantId == _currentTenant.Id && x.PhoneNumber == newPhone && x.Id != customer.Id, ct);
            if (!taken) customer.PhoneNumber = newPhone;
        }

        await _customerRepo.UpdateAsync(customer, autoSave: true, cancellationToken: ct);

        return MapToDto(customer, profile);
    }

    public async Task<ProfileStatsDto> GetStatsAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        // knowledgeLearned = số bài học đã hoàn thành.
        var knowledgeLearned = await _progressRepo.CountAsync(
            x => x.CustomerId == customer.Id && x.IsCompleted, ct);

        // Accuracy is derived exclusively from server-scored, completed sessions.
        // Do not use any score/accuracy value supplied by the Mini App client.
        var sessionQ = await _sessionRepo.GetQueryableAsync();
        var totals = await AsyncExecuter.FirstOrDefaultAsync(
            sessionQ.Where(x => x.CustomerId == customer.Id && x.IsFinished && x.TotalQuestions > 0)
                    .GroupBy(_ => 1)
                    .Select(g => new
                    {
                        Correct = g.Sum(x => x.CorrectCount),
                        Questions = g.Sum(x => x.TotalQuestions)
                    }), ct);

        var accuracy = totals == null || totals.Questions == 0
            ? 0
            : (int)decimal.Round((decimal)totals.Correct * 100 / totals.Questions, 0, MidpointRounding.AwayFromZero);

        return new ProfileStatsDto
        {
            Points = (int)decimal.Round(customer.BonusPoint),
            KnowledgeLearned = knowledgeLearned,
            AccuracyPercent = accuracy
        };
    }

    public async Task<List<LearningHistoryItemDto>> GetLearningHistoryAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        var progressQ = await _progressRepo.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            progressQ.Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.LastViewedAt), ct);

        if (rows.Count == 0) return new List<LearningHistoryItemDto>();

        // Lấy tên bài học cho các ProductId liên quan (1 query).
        var productIds = rows.Select(x => x.ProductId).Distinct().ToList();
        var prodQ = await _productRepo.GetQueryableAsync();
        var products = await AsyncExecuter.ToListAsync(
            prodQ.Where(p => productIds.Contains(p.Id)).Select(p => new { p.Id, p.Name, p.ThumbnailUrl }), ct);
        var nameById = products.ToDictionary(x => x.Id, x => x.Name);

        return rows.Select(x => new LearningHistoryItemDto
        {
            ProductId = x.ProductId,
            ProductName = nameById.TryGetValue(x.ProductId, out var n) ? n : string.Empty,
            ThumbnailUrl = NormalizeMediaUrl(products.FirstOrDefault(p => p.Id == x.ProductId)?.ThumbnailUrl),
            ProgressPercent = x.ProgressPercent,
            LastViewedAt = x.LastViewedAt
        }).ToList();
    }

    public async Task<List<PointHistoryItemDto>> GetPointHistoryAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        // Lịch sử điểm = lịch sử chơi game trả lời câu hỏi (vd "Thử thách tốc độ") đã KẾT THÚC
        // và ĐẠT YÊU CẦU (đúng >= 1/2 tổng số câu, khớp ví dụ >=5/10). Điểm game cộng vào
        // Customer.BonusPoint khi finish (AD-2); mỗi phiên đạt yêu cầu là 1 mục lịch sử điểm.
        var sessions = await _sessionRepo.GetQueryableAsync();
        var games = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame, Guid>>().GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            from s in sessions
            join g in games on s.GameId equals g.Id
            where s.CustomerId == customer.Id && s.IsFinished && s.FinishedAt != null
                  && s.TotalQuestions > 0 && s.CorrectCount * 2 >= s.TotalQuestions
            orderby s.FinishedAt descending, s.Id
            select new { s.Id, GameName = g.Name, s.Score, s.CorrectCount, s.TotalQuestions, s.FinishedAt }, ct);

        return rows.Select(x => new PointHistoryItemDto
        {
            Id = x.Id,
            SourceName = $"{x.GameName} (đúng {x.CorrectCount}/{x.TotalQuestions})",
            PointDelta = x.Score,
            CreatedAt = x.FinishedAt!.Value
        }).ToList();
    }

    public async Task<List<RewardHistoryItemDto>> GetRewardHistoryAsync(string phone, CancellationToken ct = default)
    {
        var (customer, _) = await ResolveAsync(phone, ct);

        var q = await _rewardHistoryRepo.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            q.Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.CreationTime), ct);

        var games = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame,Guid>>().GetQueryableAsync();

        // Nguồn 1: tự đổi quà ngay sau 1 phiên game (SessionId) — luồng cũ.
        var sessionIds = rows.Where(x=>x.SessionId.HasValue).Select(x=>x.SessionId!.Value).ToList();
        var sessions = await _sessionRepo.GetQueryableAsync();
        var sessionOrigins = await AsyncExecuter.ToListAsync(from s in sessions join g in games on s.GameId equals g.Id
            where sessionIds.Contains(s.Id) && s.CustomerId == customer.Id select new { s.Id, g.Name }, ct);

        // Nguồn 2: trao giải qua tính năng "Trao giải trúng thưởng" (WinnerId -> HlgRankingWinner.GameId) — luồng mới.
        var winnerIds = rows.Where(x=>x.WinnerId.HasValue).Select(x=>x.WinnerId!.Value).ToList();
        var winners = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgRankingWinner,Guid>>().GetQueryableAsync();
        var winnerOrigins = await AsyncExecuter.ToListAsync(from w in winners join g in games on w.GameId equals g.Id
            where winnerIds.Contains(w.Id) && w.CustomerId == customer.Id select new { w.Id, g.Name }, ct);

        return rows.Select(x => new RewardHistoryItemDto
        {
            GameName = sessionOrigins.FirstOrDefault(s=>s.Id==x.SessionId)?.Name
                ?? winnerOrigins.FirstOrDefault(w=>w.Id==x.WinnerId)?.Name,
            Id = x.Id,
            RewardName = x.RewardName,
            PointDelta = x.PointDelta,
            Status = HlgEnumMapper.RewardHistoryStatusToString(x.Status),
            CreatedAt = x.CreationTime
        }).ToList();
    }

    public async Task<List<GameHistoryDto>> GetGameHistoryAsync(string phone, int skip = 0, int take = 50)
    {
        var (customer, _) = await ResolveAsync(phone, default);
        var sessions = await _sessionRepo.GetQueryableAsync();
        var games = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame,Guid>>().GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync((from s in sessions join g in games on s.GameId equals g.Id
            where s.CustomerId == customer.Id && s.IsFinished && s.FinishedAt != null
            orderby s.FinishedAt descending, s.Id select new { s.Id, s.GameId, GameName = g.Name, s.Score, s.StartedAt, s.FinishedAt }).Skip(Math.Max(0,skip)).Take(Math.Clamp(take,1,100)));
        return rows.Select(x=>new GameHistoryDto { Id=x.Id,GameId=x.GameId,GameName=x.GameName,Score=x.Score,FinishedAt=x.FinishedAt!.Value,DurationSeconds=(int)Math.Max(0,(x.FinishedAt.Value-x.StartedAt).TotalSeconds) }).ToList();
    }
    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Tìm Customer theo phone + đảm bảo có HlgUserProfile (tạo nếu thiếu).</summary>
    private async Task<(Customer customer, HlgUserProfile profile)> ResolveAsync(string phone, CancellationToken ct)
    {
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new UserFriendlyException("Thiếu số điện thoại");

        var customer = await FindRegistrationCustomerAsync(RequireRegistrationPhone(normalized), ct)
            ?? throw new UserFriendlyException("Không tìm thấy khách hàng. Vui lòng đăng ký trước.");

        var profile = await FindRegistrationProfileAsync(customer.Id, ct);
        if (profile == null)
        {
            // Reads may create legacy profiles. Coordinate with registration to avoid duplicates.
            using var uow = _registrationUow.Begin(requiresNew: true, isTransactional: true);
            await _registrationLock.AcquireAsync(ct);
            profile = await FindRegistrationProfileAsync(customer.Id, ct);
            if (profile == null)
            {
                profile = new HlgUserProfile(GuidGenerator.Create(), customer.Id, _currentTenant.Id)
                {
                    ZaloId = customer.ZaloUserId,
                    IsRegistered = false
                };
                profile = await _profileRepo.InsertAsync(profile, autoSave: true, cancellationToken: ct);
            }
            await uow.CompleteAsync(ct);
        }

        return (customer, profile);
    }

    private GamificationUserDto MapToDto(Customer c, HlgUserProfile p)
    {
        return new GamificationUserDto
        {
            Id = p.Id,
            ZaloId = c.ZaloUserId ?? p.ZaloId,
            FullName = c.FullName,
            Phone = c.PhoneNumber,
            PharmaPhone = p.PharmaPhone,
            CustomerCode = c.CustomerCode,
            DmsCustomerCode = p.DmsCustomerCode,
            Gender = HlgEnumMapper.GenderByteToString(c.Gender),
            Birthday = HlgEnumMapper.DateToIso(c.DateOfBirth),
            Address = c.Address,
            PharmacyCode = p.PharmacyCode,
            VgaCode = p.PharmacyCode,
            AvatarUrl = NormalizeMediaUrl(c.AvatarUrl),
            CustomerType = HlgEnumMapper.CustomerTypeToString(p.CustomerType),
            Points = (int)decimal.Round(c.BonusPoint),
            IsRegistered = p.IsRegistered,
            CreatedAt = p.CreationTime
        };
    }

    private void ValidatePharmacyCode(string? code) { if (code?.Length > 100) throw new UserFriendlyException(L["Hlg:PharmacyCodeTooLong"]); }
    private void ValidateCustomerType(string? type) { if (type != null && HlgEnumMapper.CustomerTypeFromString(type) == null) throw new UserFriendlyException(L["Hlg:InvalidCustomerType"]); }
    private static string? NormalizePhone(string? phone) => HlgRegistrationRules.NormalizePhone(phone);

    private static string? NullIfBlank(string? value)
        => string.IsNullOrWhiteSpace(value) ? null : value.Trim();

    private string? NormalizeMediaUrl(string? url)
        => ImageHelper.NormalizeThumb(_configuration, url);

    private async Task<string> GenerateCustomerCodeAsync()
    {
        // SQL uniqueness also reserves codes on soft-deleted customers.
        using var includeDeleted = DataFilter.Disable<ISoftDelete>();
        const string prefix = "HLGKH";
        var queryable = await _customerRepo.GetQueryableAsync();

        var maxNumber = 0;
        var codes = await AsyncExecuter.ToListAsync(queryable
            .Where(c => c.TenantId == _currentTenant.Id && c.CustomerCode != null && c.CustomerCode.StartsWith(prefix))
            .Select(c => c.CustomerCode!));
        foreach (var code in codes)
        {
            var numberPart = code.Substring(prefix.Length);
            if (int.TryParse(numberPart, System.Globalization.NumberStyles.None, System.Globalization.CultureInfo.InvariantCulture, out var n) && n > maxNumber)
                maxNumber = n;
        }

        var next = maxNumber + 1;
        var candidate = $"{prefix}{next.ToString("D6", System.Globalization.CultureInfo.InvariantCulture)}";

        while (await _customerRepo.AnyAsync(c => c.TenantId == _currentTenant.Id && c.CustomerCode == candidate))
        {
            next++;
            candidate = $"{prefix}{next.ToString("D6", System.Globalization.CultureInfo.InvariantCulture)}";
        }

        return candidate;
    }

    private static DateTime? ParseDate(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;
        string[] formats = { "yyyy-MM-dd", "dd/MM/yyyy", "MM/dd/yyyy", "yyyy-MM-ddTHH:mm:ss", "dd-MM-yyyy" };
        if (DateTime.TryParseExact(raw.Trim(), formats, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out var d))
            return d.Date;
        if (DateTime.TryParse(raw.Trim(), System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out d))
            return d.Date;
        return null;
    }
}
