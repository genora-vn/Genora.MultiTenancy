using Genora.MultiTenancy.AppDtos.HoaLinh.Blouse;
using Genora.MultiTenancy.DomainModels.AppHlBlouse;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// AppService phục vụ Zalo Mini App "Đăng ký nhận áo Blouse" (public/anonymous).
/// Nhận payload động (danh sách áo tặng + áo đổi điểm), validate tồn kho, ghi nhận thông tin đăng ký.
/// Lưu ý: KHÔNG có nghiệp vụ trừ điểm tích lũy — chỉ ghi nhận số điểm quy đổi (TotalPointsUsed) để tham khảo.
/// </summary>
[AllowAnonymous]
[RemoteService(false)]
[DisableValidation]
public class MiniAppHlBlouseService : ApplicationService, IMiniAppHlBlouseService
{
    private readonly IRepository<HlBlouseCampaign, Guid> _campaignRepo;
    private readonly IRepository<HlBlouseSize, Guid> _sizeRepo;
    private readonly IRepository<HlBlouseRegistration, Guid> _registrationRepo;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly ICurrentTenant _currentTenant;
    private readonly IConfiguration _configuration;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public MiniAppHlBlouseService(
        IRepository<HlBlouseCampaign, Guid> campaignRepo,
        IRepository<HlBlouseSize, Guid> sizeRepo,
        IRepository<HlBlouseRegistration, Guid> registrationRepo,
        IUnitOfWorkManager uowManager,
        ICurrentTenant currentTenant,
        IConfiguration configuration,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _campaignRepo = campaignRepo;
        _sizeRepo = sizeRepo;
        _registrationRepo = registrationRepo;
        _uowManager = uowManager;
        _currentTenant = currentTenant;
        _configuration = configuration;
        _l = l;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    /// <summary>Tạo UserFriendlyException kèm mã lỗi (Code) = key localization để FE mini app handle theo code.</summary>
    private UserFriendlyException Err(string code, params object[] args)
        => new(_l[code, args], code: code);

    // ========================================================================
    // Cấu hình + danh mục size
    // ========================================================================
    public async Task<HlBlouseConfigDto> GetConfigAsync()
    {
        var campaign = await GetActiveCampaignOrNullAsync();

        var sizeQueryable = await _sizeRepo.GetQueryableAsync();
        var sizes = await AsyncExecuter.ToListAsync(
            sizeQueryable.Where(x => x.IsActive).OrderBy(x => x.DisplayOrder).ThenBy(x => x.SizeCode));

        var dto = new HlBlouseConfigDto
        {
            IsActive = campaign?.IsActive ?? false,
            ProgramName = campaign?.ProgramName,
            IntroductionHtml = campaign?.IntroductionHtml,
            FreeShirtLimit = campaign?.FreeShirtLimit ?? 0,
            PointsPerShirt = campaign?.PointsPerShirt ?? 0,
            MaxExchangeShirt = campaign?.MaxExchangeShirt ?? 0,
            // Trả full URL banner cho mini app (path tương đối /uploads/... -> App:AppUrl + path).
            SizeChartImageUrl = ImageHelper.NormalizeThumb(_configuration, campaign?.SizeChartImageUrl),
            StartTime = campaign?.StartTime,
            EndTime = campaign?.EndTime,
            MaleSizes = sizes.Where(x => x.Style == HlBlouseStyle.Male).Select(MapSize).ToList(),
            FemaleSizes = sizes.Where(x => x.Style == HlBlouseStyle.Female).Select(MapSize).ToList()
        };
        return dto;
    }

    // ========================================================================
    // Đăng ký (payload động)
    // ========================================================================
    public async Task<HlBlouseRegistrationDto> RegisterAsync(HlBlouseRegisterRequest request, CancellationToken ct = default)
    {
        if (request == null)
            throw Err("HlBlouse:InvalidPayload");
        if (string.IsNullOrWhiteSpace(request.CustomerPhone))
            throw Err("HlBlouse:PhoneRequired");
        if (request.Items == null || request.Items.Count == 0)
            throw Err("HlBlouse:NoItems");

        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        // 1) Cấu hình chương trình
        var campaign = await GetActiveCampaignOrNullAsync()
            ?? throw Err("HlBlouse:CampaignInactive");
        if (!campaign.IsActive)
            throw Err("HlBlouse:CampaignInactive");
        var now = DateTime.Now;
        if (campaign.StartTime.HasValue && now < campaign.StartTime.Value)
            throw Err("HlBlouse:CampaignNotStarted");
        if (campaign.EndTime.HasValue && now > campaign.EndTime.Value)
            throw Err("HlBlouse:CampaignEnded");

        // 2) Chặn trùng: mỗi (số điện thoại + mã khách hàng/chi nhánh) chỉ được đăng ký 1 lần.
        //    Cùng SĐT nhưng khác mã KH (chi nhánh khác) vẫn được đăng ký. Bỏ qua đơn đã Hủy/Từ chối.
        await EnsureBranchNotRegisteredAsync(request.CustomerPhone!, request.CustomerCode);

        // 3) Nạp toàn bộ size active để tra cứu + validate tồn kho
        var sizeQueryable = await _sizeRepo.GetQueryableAsync();
        var allSizes = await AsyncExecuter.ToListAsync(sizeQueryable.Where(x => x.IsActive));

        // 4) Gộp dòng theo (type, style, size) để cộng dồn số lượng cùng size
        var normalized = new List<(HlBlouseItemType Type, HlBlouseSize Size, int Quantity)>();
        foreach (var item in request.Items)
        {
            if (item.Quantity <= 0)
                continue; // bỏ qua dòng số lượng 0

            var size = ResolveSize(allSizes, item);
            if (size == null)
                throw Err("HlBlouse:SizeNotFound", $"{item.Style} {item.SizeCode}");

            normalized.Add((item.ItemType, size, item.Quantity));
        }

        if (normalized.Count == 0)
            throw new UserFriendlyException(_l["HlBlouse:NoItems"]);

        // 4-6) Tính tổng hợp + validate (tồn kho, giới hạn áo tặng/đổi) bằng logic thuần
        HlBlouseValidator.Summary summary;
        try
        {
            summary = HlBlouseValidator.BuildSummary(
                normalized.Select(n => new HlBlouseValidator.Line
                {
                    ItemType = n.Type,
                    Style = n.Size.Style,
                    SizeKey = n.Size.Id.ToString(),
                    Quantity = n.Quantity,
                    Stock = n.Size.StockQuantity,
                    SizeLabel = $"{n.Size.Style} {n.Size.SizeCode}"
                }),
                campaign.FreeShirtLimit,
                campaign.PointsPerShirt,
                campaign.MaxExchangeShirt);
        }
        catch (BlouseValidationError err)
        {
            throw new UserFriendlyException(_l[err.Code, err.Args]);
        }

        var freeQty = summary.FreeQuantity;
        var exchangeQty = summary.ExchangeQuantity;
        var totalPoints = summary.TotalPointsUsed; // chỉ ghi nhận để tham khảo, KHÔNG trừ điểm

        // 7) Tạo đơn + dòng
        var registration = new HlBlouseRegistration(
            GuidGenerator.Create(),
            await GenerateCodeAsync(),
            _currentTenant.Id)
        {
            CustomerCode = request.CustomerCode,
            CustomerName = request.CustomerName,
            CustomerPhone = request.CustomerPhone,
            ZaloUserId = request.ZaloUserId,
            ReceiverName = request.ReceiverName,
            DeliveryAddress = request.DeliveryAddress,
            BusinessType = request.BusinessType,
            BusinessTypeName = request.BusinessTypeName,
            StoreName = request.StoreName,
            PrintedName = request.PrintedName,
            Note = request.Note,
            FreeQuantity = freeQty,
            ExchangeQuantity = exchangeQty,
            TotalQuantity = freeQty + exchangeQty,
            TotalPointsUsed = totalPoints,
            Status = HlBlouseRegistrationStatus.Pending
        };

        foreach (var n in normalized)
        {
            var pointsPerItem = n.Type == HlBlouseItemType.Exchange ? campaign.PointsPerShirt : 0;
            registration.Items.Add(new HlBlouseRegistrationItem(
                GuidGenerator.Create(),
                registration.Id,
                n.Type,
                n.Size.Style,
                n.Size.SizeCode,
                n.Quantity,
                pointsPerItem,
                _currentTenant.Id)
            {
                SizeId = n.Size.Id,
                WeightRange = n.Size.WeightRange
            });
        }

        await _registrationRepo.InsertAsync(registration, autoSave: true);

        // 8) Trừ tồn kho theo tổng số lượng mỗi size (summary.StockNeededBySize keyed theo size Id)
        var sizeById = normalized
            .Select(n => n.Size)
            .GroupBy(s => s.Id)
            .ToDictionary(g => g.Key, g => g.First());
        foreach (var kv in summary.StockNeededBySize)
        {
            if (Guid.TryParse(kv.Key, out var sizeId) && sizeById.TryGetValue(sizeId, out var size))
            {
                size.StockQuantity -= kv.Value;
                await _sizeRepo.UpdateAsync(size, autoSave: false);
            }
        }

        await uow.CompleteAsync(ct);

        return MapToDto(registration);
    }

    // ========================================================================
    // Lịch sử đăng ký của khách
    // ========================================================================
    public async Task<ListResultDto<HlBlouseRegistrationDto>> GetMyRegistrationsAsync(string phone, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(phone))
            throw new UserFriendlyException(_l["HlBlouse:PhoneRequired"]);

        var queryable = await _registrationRepo.WithDetailsAsync(x => x.Items);
        var list = await AsyncExecuter.ToListAsync(
            queryable.Where(x => x.CustomerPhone == phone).OrderByDescending(x => x.CreationTime));

        return new ListResultDto<HlBlouseRegistrationDto>(list.Select(MapToDto).ToList());
    }

    // ========================================================================
    // Helpers
    // ========================================================================
    private async Task<HlBlouseCampaign?> GetActiveCampaignOrNullAsync()
    {
        var queryable = await _campaignRepo.GetQueryableAsync();
        return await AsyncExecuter.FirstOrDefaultAsync(
            queryable.Where(x => x.IsActive).OrderByDescending(x => x.CreationTime));
    }

    /// <summary>
    /// Chặn trùng: mỗi (số điện thoại + mã khách hàng/chi nhánh) chỉ được đăng ký 1 lần.
    /// Cùng SĐT nhưng khác mã KH (chi nhánh khác) vẫn được đăng ký. Bỏ qua đơn đã Hủy/Từ chối.
    /// </summary>
    private async Task EnsureBranchNotRegisteredAsync(string phone, string? customerCode)
    {
        var code = string.IsNullOrWhiteSpace(customerCode) ? null : customerCode.Trim();
        var queryable = await _registrationRepo.GetQueryableAsync();
        var exists = await AsyncExecuter.AnyAsync(queryable.Where(x =>
            x.CustomerPhone == phone &&
            x.CustomerCode == code &&
            x.Status != HlBlouseRegistrationStatus.Cancelled &&
            x.Status != HlBlouseRegistrationStatus.Rejected));

        if (exists)
            throw Err("HlBlouse:BranchAlreadyRegistered");
    }

    private static HlBlouseSize? ResolveSize(List<HlBlouseSize> sizes, HlBlouseRegisterItemDto item)
    {
        if (item.SizeId.HasValue)
            return sizes.FirstOrDefault(x => x.Id == item.SizeId.Value && x.Style == item.Style);
        if (!string.IsNullOrWhiteSpace(item.SizeCode))
            return sizes.FirstOrDefault(x =>
                x.Style == item.Style &&
                x.SizeCode.Equals(item.SizeCode, StringComparison.OrdinalIgnoreCase));
        return null;
    }

    private async Task<string> GenerateCodeAsync()
    {
        var queryable = await _registrationRepo.GetQueryableAsync();
        var todayPrefix = "HLBL-" + DateTime.Now.ToString("yyMMdd");
        var countToday = await AsyncExecuter.CountAsync(
            queryable.Where(x => x.RegistrationCode.StartsWith(todayPrefix)));
        return $"{todayPrefix}{(countToday + 1):D4}";
    }

    private static HlBlouseSizeDto MapSize(HlBlouseSize s) => new()
    {
        Id = s.Id,
        Style = s.Style,
        SizeCode = s.SizeCode,
        WeightRange = s.WeightRange,
        StockQuantity = s.StockQuantity,
        DisplayOrder = s.DisplayOrder,
        IsActive = s.IsActive,
        InStock = s.IsActive && s.StockQuantity > 0
    };

    private static HlBlouseRegistrationDto MapToDto(HlBlouseRegistration e) => new()
    {
        Id = e.Id,
        RegistrationCode = e.RegistrationCode,
        CustomerCode = e.CustomerCode,
        CustomerName = e.CustomerName,
        CustomerPhone = e.CustomerPhone,
        ZaloUserId = e.ZaloUserId,
        ReceiverName = e.ReceiverName,
        DeliveryAddress = e.DeliveryAddress,
        BusinessType = e.BusinessType,
        BusinessTypeName = e.BusinessTypeName,
        StoreName = e.StoreName,
        PrintedName = e.PrintedName,
        Note = e.Note,
        FreeQuantity = e.FreeQuantity,
        ExchangeQuantity = e.ExchangeQuantity,
        TotalQuantity = e.TotalQuantity,
        TotalPointsUsed = e.TotalPointsUsed,
        Status = e.Status,
        InternalNote = e.InternalNote,
        CreationTime = e.CreationTime,
        ProcessedAt = e.ProcessedAt,
        Items = e.Items?.Select(i => new HlBlouseRegistrationItemResultDto
        {
            Id = i.Id,
            ItemType = i.ItemType,
            Style = i.Style,
            SizeId = i.SizeId,
            SizeCode = i.SizeCode,
            WeightRange = i.WeightRange,
            Quantity = i.Quantity,
            PointsPerItem = i.PointsPerItem,
            TotalPoints = i.TotalPoints
        }).ToList() ?? new List<HlBlouseRegistrationItemResultDto>()
    };
}
