using Genora.MultiTenancy.AppDtos.HoaLinh.Blouse;
using Genora.MultiTenancy.DomainModels.AppHlBlouse;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// AppService quản trị (Admin) chương trình "Đăng ký nhận áo Blouse".
/// Dual permission Tenant/Host — không dùng [Authorize] cứng (theo RULES).
/// </summary>
public class HlBlouseAdminAppService : ApplicationService, IHlBlouseAdminAppService
{
    private readonly IRepository<HlBlouseRegistration, Guid> _registrationRepo;
    private readonly IRepository<HlBlouseCampaign, Guid> _campaignRepo;
    private readonly IRepository<HlBlouseSize, Guid> _sizeRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAuthorizationService _authService;

    public HlBlouseAdminAppService(
        IRepository<HlBlouseRegistration, Guid> registrationRepo,
        IRepository<HlBlouseCampaign, Guid> campaignRepo,
        IRepository<HlBlouseSize, Guid> sizeRepo,
        ICurrentTenant currentTenant,
        IAuthorizationService authService)
    {
        _registrationRepo = registrationRepo;
        _campaignRepo = campaignRepo;
        _sizeRepo = sizeRepo;
        _currentTenant = currentTenant;
        _authService = authService;
    }

    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.Id.HasValue ? tenantPerm : hostPerm;

    private async Task CheckPermissionAsync(string tenantPerm, string hostPerm)
    {
        var perm = P(tenantPerm, hostPerm);
        var result = await _authService.AuthorizeAsync(perm);
        if (!result.Succeeded)
            throw new Volo.Abp.Authorization.AbpAuthorizationException($"Permission denied: {perm}");
    }

    // ========================================================================
    // Đơn đăng ký
    // ========================================================================
    public async Task<PagedResultDto<HlBlouseRegistrationDto>> GetListAsync(HlBlouseRegistrationFilterDto input)
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Default,
            MultiTenancyPermissions.HostAppHlBlouse.Default);

        HlSalesQuery.ValidateDates(input.DateFrom, input.DateTo);

        var queryable = await _registrationRepo.WithDetailsAsync(x => x.Items);

        if (!string.IsNullOrWhiteSpace(input.Filter))
        {
            var f = input.Filter.Trim();
            queryable = queryable.Where(x =>
                x.RegistrationCode.Contains(f) ||
                (x.CustomerCode ?? "").Contains(f) ||
                (x.CustomerName ?? "").Contains(f) ||
                (x.CustomerPhone ?? "").Contains(f) ||
                (x.PrintedName ?? "").Contains(f) ||
                (x.StoreName ?? "").Contains(f));
        }
        if (input.Status.HasValue)
            queryable = queryable.Where(x => x.Status == input.Status.Value);
        if (input.DateFrom.HasValue)
        {
            var from = input.DateFrom.Value.Date;
            queryable = queryable.Where(x => x.CreationTime >= from);
        }
        if (input.DateTo.HasValue)
        {
            var until = input.DateTo.Value.Date.AddDays(1);
            queryable = queryable.Where(x => x.CreationTime < until);
        }

        var totalCount = await AsyncExecuter.CountAsync(queryable);
        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        return new PagedResultDto<HlBlouseRegistrationDto>(totalCount, items.Select(MapToDto).ToList());
    }

    public async Task<HlBlouseRegistrationDto> GetAsync(Guid id)
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Default,
            MultiTenancyPermissions.HostAppHlBlouse.Default);

        var queryable = await _registrationRepo.WithDetailsAsync(x => x.Items);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == id))
            ?? throw new EntityNotFoundException(typeof(HlBlouseRegistration), id);
        return MapToDto(entity);
    }

    public async Task<HlBlouseRegistrationDto> UpdateStatusAsync(HlBlouseUpdateStatusDto input)
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Edit,
            MultiTenancyPermissions.HostAppHlBlouse.Edit);

        var queryable = await _registrationRepo.WithDetailsAsync(x => x.Items);
        var entity = await AsyncExecuter.FirstOrDefaultAsync(queryable.Where(x => x.Id == input.Id))
            ?? throw new EntityNotFoundException(typeof(HlBlouseRegistration), input.Id);

        entity.Status = input.Status;
        if (!string.IsNullOrWhiteSpace(input.InternalNote))
            entity.InternalNote = input.InternalNote;
        entity.ProcessedBy = CurrentUser.Id;
        entity.ProcessedAt = DateTime.Now;

        await _registrationRepo.UpdateAsync(entity, autoSave: true);
        return MapToDto(entity);
    }

    // ========================================================================
    // Cấu hình chương trình
    // ========================================================================
    public async Task<HlBlouseCampaignDto?> GetCampaignAsync()
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Default,
            MultiTenancyPermissions.HostAppHlBlouse.Default);

        var queryable = await _campaignRepo.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.OrderByDescending(x => x.IsActive).ThenByDescending(x => x.CreationTime));
        return entity == null ? null : MapCampaign(entity);
    }

    public async Task<HlBlouseCampaignDto> SaveCampaignAsync(HlBlouseCampaignSaveDto input)
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Edit,
            MultiTenancyPermissions.HostAppHlBlouse.Edit);

        if (string.IsNullOrWhiteSpace(input.ProgramName))
            throw new UserFriendlyException("Tên chương trình là bắt buộc.");
        if (input.FreeShirtLimit < 0 || input.PointsPerShirt < 0 || input.MaxExchangeShirt < 0)
            throw new UserFriendlyException("Giá trị cấu hình không hợp lệ.");

        var queryable = await _campaignRepo.GetQueryableAsync();
        var entity = await AsyncExecuter.FirstOrDefaultAsync(
            queryable.OrderByDescending(x => x.CreationTime));

        if (entity == null)
        {
            entity = new HlBlouseCampaign(GuidGenerator.Create(), input.ProgramName, _currentTenant.Id);
            ApplyCampaign(entity, input);
            await _campaignRepo.InsertAsync(entity, autoSave: true);
        }
        else
        {
            ApplyCampaign(entity, input);
            await _campaignRepo.UpdateAsync(entity, autoSave: true);
        }
        return MapCampaign(entity);
    }

    // ========================================================================
    // Danh mục size
    // ========================================================================
    public async Task<ListResultDto<HlBlouseSizeDto>> GetSizesAsync()
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Default,
            MultiTenancyPermissions.HostAppHlBlouse.Default);

        var queryable = await _sizeRepo.GetQueryableAsync();
        var list = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(x => x.Style).ThenBy(x => x.DisplayOrder).ThenBy(x => x.SizeCode));
        return new ListResultDto<HlBlouseSizeDto>(list.Select(MapSize).ToList());
    }

    public async Task<HlBlouseSizeDto> CreateSizeAsync(HlBlouseSizeSaveDto input)
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Create,
            MultiTenancyPermissions.HostAppHlBlouse.Create);

        ValidateSizeInput(input);

        var entity = new HlBlouseSize(GuidGenerator.Create(), input.Style, input.SizeCode.Trim(), _currentTenant.Id);
        ApplySize(entity, input);
        await _sizeRepo.InsertAsync(entity, autoSave: true);
        return MapSize(entity);
    }

    public async Task<HlBlouseSizeDto> UpdateSizeAsync(Guid id, HlBlouseSizeSaveDto input)
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Edit,
            MultiTenancyPermissions.HostAppHlBlouse.Edit);

        ValidateSizeInput(input);

        var entity = await _sizeRepo.GetAsync(id);
        entity.Style = input.Style;
        entity.SizeCode = input.SizeCode.Trim();
        ApplySize(entity, input);
        await _sizeRepo.UpdateAsync(entity, autoSave: true);
        return MapSize(entity);
    }

    public async Task DeleteSizeAsync(Guid id)
    {
        await CheckPermissionAsync(
            MultiTenancyPermissions.AppHlBlouse.Delete,
            MultiTenancyPermissions.HostAppHlBlouse.Delete);

        await _sizeRepo.DeleteAsync(id);
    }

    // ========================================================================
    // Helpers
    // ========================================================================
    private static void ValidateSizeInput(HlBlouseSizeSaveDto input)
    {
        if (string.IsNullOrWhiteSpace(input.SizeCode))
            throw new UserFriendlyException("Mã size là bắt buộc.");
        if (input.StockQuantity < 0)
            throw new UserFriendlyException("Tồn kho không được âm.");
    }

    private static void ApplyCampaign(HlBlouseCampaign entity, HlBlouseCampaignSaveDto input)
    {
        entity.ProgramName = input.ProgramName;
        entity.IntroductionHtml = input.IntroductionHtml;
        entity.FreeShirtLimit = input.FreeShirtLimit;
        entity.PointsPerShirt = input.PointsPerShirt;
        entity.MaxExchangeShirt = input.MaxExchangeShirt;
        entity.SizeChartImageUrl = input.SizeChartImageUrl;
        entity.StartTime = input.StartTime;
        entity.EndTime = input.EndTime;
        entity.IsActive = input.IsActive;
    }

    private static void ApplySize(HlBlouseSize entity, HlBlouseSizeSaveDto input)
    {
        entity.WeightRange = input.WeightRange;
        entity.StockQuantity = input.StockQuantity;
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;
    }

    private static HlBlouseCampaignDto MapCampaign(HlBlouseCampaign e) => new()
    {
        Id = e.Id,
        ProgramName = e.ProgramName,
        IntroductionHtml = e.IntroductionHtml,
        FreeShirtLimit = e.FreeShirtLimit,
        PointsPerShirt = e.PointsPerShirt,
        MaxExchangeShirt = e.MaxExchangeShirt,
        SizeChartImageUrl = e.SizeChartImageUrl,
        StartTime = e.StartTime,
        EndTime = e.EndTime,
        IsActive = e.IsActive
    };

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
        }).ToList() ?? new System.Collections.Generic.List<HlBlouseRegistrationItemResultDto>()
    };
}
