using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlApiLogs;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Admin AppService — wrap IHlApiClientService, dual permission (host/tenant), data-level filter
/// </summary>
public class HlAdminAppService : ApplicationService, IHlAdminAppService
{
    private readonly IHlApiClientService _hlApi;
    private readonly IHlDataAccessService _dataAccess;
    private readonly ICurrentTenant _currentTenant;
    private readonly IAuthorizationService _authService;
    private readonly IRepository<HlApiLog, Guid> _apiLogRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;

    public HlAdminAppService(
        IHlApiClientService hlApi,
        IHlDataAccessService dataAccess,
        ICurrentTenant currentTenant,
        IAuthorizationService authService,
        IRepository<HlApiLog, Guid> apiLogRepo,
        IRepository<Customer, Guid> customerRepo)
    {
        _hlApi = hlApi;
        _dataAccess = dataAccess;
        _currentTenant = currentTenant;
        _authService = authService;
        _apiLogRepo = apiLogRepo;
        _customerRepo = customerRepo;
    }

    /// <summary>
    /// Dual permission helper: Host dùng HostAppHl*, Tenant dùng AppHl*
    /// </summary>
    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.Id.HasValue ? tenantPerm : hostPerm;

    private async Task CheckPermissionAsync(string tenantPerm, string hostPerm)
    {
        var perm = P(tenantPerm, hostPerm);
        var result = await _authService.AuthorizeAsync(perm);
        if (!result.Succeeded)
            throw new Volo.Abp.Authorization.AbpAuthorizationException($"Permission denied: {perm}");
    }

    #region Products

    public async Task<HlApiResult<HlPagedResponse<HlProductDto>>> GetProductsAsync(int page, int limit, string? search = null)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlProducts.Default, MultiTenancyPermissions.HostAppHlProducts.Default);
        return await _hlApi.GetProductsAsync(page, limit, search);
    }

    public async Task<HlApiResult<List<HlProductDto>>> GetProductDetailAsync(string productCode)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlProducts.Default, MultiTenancyPermissions.HostAppHlProducts.Default);
        return await _hlApi.GetProductDetailAsync(productCode);
    }

    #endregion

    #region Customers

    public async Task<HlApiResult<HlPagedResponse<HlCustomerDto>>> GetCustomersAsync(int page, int limit, string? search = null, int? source = null)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlCustomers.Default, MultiTenancyPermissions.HostAppHlCustomers.Default);

        var result = await _hlApi.GetCustomersAsync(page, limit, search);
        var hlList = (result.Success && result.Data?.Data != null) ? result.Data.Data : new List<HlCustomerDto>();

        // Data-level filter: Sales chỉ thấy KH mình phụ trách
        var dsrCode = await _dataAccess.GetCurrentUserDsrCodeAsync();
        if (!string.IsNullOrEmpty(dsrCode))
        {
            hlList = hlList
                .Where(x => string.Equals(x.DsrCode, dsrCode, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        // Lấy toàn bộ KH trong dbo.AppCustomers để merge theo CustomerCode ↔ custCode
        var queryable = await _customerRepo.GetQueryableAsync();
        var genoraCustomers = await AsyncExecuter.ToListAsync(queryable);

        var genoraByCode = genoraCustomers
            .Where(x => !string.IsNullOrWhiteSpace(x.CustomerCode))
            .GroupBy(x => x.CustomerCode!, StringComparer.OrdinalIgnoreCase)
            .ToDictionary(g => g.Key, g => g.First(), StringComparer.OrdinalIgnoreCase);

        var merged = new List<HlCustomerDto>();
        var matchedCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        // 1. Ưu tiên dữ liệu API HL DMS; enrich thêm từ AppCustomers nếu có mapping theo custCode
        foreach (var hl in hlList)
        {
            if (!string.IsNullOrWhiteSpace(hl.CustCode) && genoraByCode.TryGetValue(hl.CustCode!, out var g))
            {
                matchedCodes.Add(hl.CustCode!);
                EnrichFromGenora(hl, g);
            }
            else
            {
                // Chỉ có trên HL DMS → nguồn HoaLinh
                hl.Source = (int)CustomerSource.HoaLinh;
                hl.SourceText = GetSourceText(CustomerSource.HoaLinh);
                hl.ExistsInGenora = false;
            }
            merged.Add(hl);
        }

        // 2. Sau đó thêm KH chỉ có trong Genora DB (chưa map với API)
        foreach (var g in genoraCustomers)
        {
            if (!string.IsNullOrWhiteSpace(g.CustomerCode) && matchedCodes.Contains(g.CustomerCode!))
                continue;

            merged.Add(MapGenoraToDto(g));
        }

        // 3. Filter theo nguồn (nếu có)
        if (source.HasValue)
            merged = merged.Where(x => x.Source == source.Value).ToList();

        var paged = new HlPagedResponse<HlCustomerDto>
        {
            TotalRecords = merged.Count,
            Page = page,
            Limit = limit,
            TotalPages = 1,
            Data = merged
        };

        return HlApiResult<HlPagedResponse<HlCustomerDto>>.Ok(paged);
    }

    /// <summary>Enrich DTO từ API bằng dữ liệu Genora (chỉ bổ sung trường API thiếu). Nguồn = HoaLinh (đã có bên DMS).</summary>
    private void EnrichFromGenora(HlCustomerDto hl, Customer g)
    {
        hl.CustName ??= g.FullName;
        hl.CustPhone ??= g.PhoneNumber;
        hl.Phone ??= g.PhoneNumber;
        hl.Address ??= g.Address;
        hl.Source = (int)g.CustomerSource;
        hl.SourceText = GetSourceText(g.CustomerSource);
        hl.ExistsInGenora = true;
    }

    /// <summary>Map bản ghi Genora (dbo.AppCustomers) sang HlCustomerDto.</summary>
    private HlCustomerDto MapGenoraToDto(Customer g)
    {
        return new HlCustomerDto
        {
            CustCode = g.CustomerCode,
            CustName = g.FullName,
            CustPhone = g.PhoneNumber,
            Phone = g.PhoneNumber,
            Address = g.Address,
            Source = (int)g.CustomerSource,
            SourceText = GetSourceText(g.CustomerSource),
            ExistsInGenora = true
        };
    }

    private static string GetSourceText(CustomerSource source) => source switch
    {
        CustomerSource.ZaloMiniApp => "Genora (Mini App)",
        CustomerSource.HoaLinh => "Hoa Linh (DMS)",
        CustomerSource.Manual => "Nhập tay",
        CustomerSource.Extent => "Import",
        CustomerSource.Other => "Khác",
        _ => "Khác"
    };

    public async Task<HlApiResult<List<HlCustomerDto>>> GetCustomerDetailAsync(string phone)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlCustomers.Default, MultiTenancyPermissions.HostAppHlCustomers.Default);
        return await _hlApi.GetCustomerDetailAsync(phone);
    }

    public async Task<HlApiResult<List<HlCustomerDto>>> GetCustomerByPhoneAsync(string phone)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlCustomers.Default, MultiTenancyPermissions.HostAppHlCustomers.Default);
        return await _hlApi.GetCustomerByPhoneAsync(phone);
    }

    #endregion

    #region Salemans

    public async Task<HlApiResult<HlPagedResponse<HlSalemanDto>>> GetSalemansAsync(int page, int limit)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlCustomers.Default, MultiTenancyPermissions.HostAppHlCustomers.Default);
        return await _hlApi.GetSalemansAsync(page, limit);
    }

    public async Task<HlApiResult<List<HlSalemanDto>>> GetSalemanDetailAsync(string dsrCode)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlCustomers.Default, MultiTenancyPermissions.HostAppHlCustomers.Default);
        return await _hlApi.GetSalemanDetailAsync(dsrCode);
    }

    #endregion

    #region Orders

    public async Task<HlApiResult<HlPagedResponse<HlOrderDetailDto>>> GetOrdersAsync(int page, int limit, string? customerCode = null)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);
        return await _hlApi.GetOrdersAsync(page, limit, customerCode);
    }

    public async Task<HlApiResult<List<HlOrderDetailDto>>> GetOrderDetailAsync(string orderNumber)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);
        return await _hlApi.GetOrderDetailAsync(orderNumber);
    }

    #endregion

    #region Campaigns

    public async Task<HlApiResult<HlPagedResponse<HlCampaignDto>>> GetCampaignsAsync(int page, int limit)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlLoyalty.Default, MultiTenancyPermissions.HostAppHlLoyalty.Default);
        return await _hlApi.GetCampaignsAsync(page, limit);
    }

    public async Task<HlApiResult<List<HlCampaignDto>>> GetCampaignDetailAsync(string custCode)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlLoyalty.Default, MultiTenancyPermissions.HostAppHlLoyalty.Default);
        return await _hlApi.GetCampaignDetailAsync(custCode);
    }

    #endregion

    #region Brands

    public async Task<HlApiResult<HlPagedResponse<HlBrandDto>>> GetBrandsAsync(int page, int limit)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlProducts.Default, MultiTenancyPermissions.HostAppHlProducts.Default);
        return await _hlApi.GetBrandsAsync(page, limit);
    }

    public async Task<HlApiResult<List<HlBrandDto>>> GetBrandDetailAsync(string brandCode)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlProducts.Default, MultiTenancyPermissions.HostAppHlProducts.Default);
        return await _hlApi.GetBrandDetailAsync(brandCode);
    }

    public async Task<HlApiResult<List<HlProductByBrandDto>>> GetProductsByBrandAsync(string brandCode)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlProducts.Default, MultiTenancyPermissions.HostAppHlProducts.Default);
        return await _hlApi.GetProductsByBrandAsync(brandCode);
    }

    #endregion

    #region Product Groups

    public async Task<HlApiResult<HlPagedResponse<HlProductGroupDto>>> GetProductGroupsAsync(int page, int limit)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlProducts.Default, MultiTenancyPermissions.HostAppHlProducts.Default);
        return await _hlApi.GetProductGroupsAsync(page, limit);
    }

    public async Task<HlApiResult<List<HlProductGroupDto>>> GetProductGroupDetailAsync(string code)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlProducts.Default, MultiTenancyPermissions.HostAppHlProducts.Default);
        return await _hlApi.GetProductGroupDetailAsync(code);
    }

    #endregion

    #region Order Headers

    public async Task<HlApiResult<HlPagedResponse<HlOrderHeaderDto>>> GetOrderHeadersAsync(int page, int limit)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);
        return await _hlApi.GetOrderHeadersAsync(page, limit);
    }

    public async Task<HlApiResult<List<HlOrderHeaderDto>>> GetOrderHeaderDetailAsync(string orderNumber)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);
        return await _hlApi.GetOrderHeaderDetailAsync(orderNumber);
    }

    #endregion

    #region Master Data

    public async Task<HlApiResult<HlPagedResponse<HlMasterOrderStatusDto>>> GetMasterOrderStatusAsync(int page, int limit)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlOrders.Default, MultiTenancyPermissions.HostAppHlOrders.Default);
        return await _hlApi.GetMasterOrderStatusAsync(page, limit);
    }

    #endregion

    #region API Logs

    public async Task<HlApiResult<HlPagedResponse<HlApiLogDto>>> GetApiLogsAsync(int page, int limit, string? dataType = null, bool? isError = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlApiLogs.Default, MultiTenancyPermissions.HostAppHlApiLogs.Default);

        var queryable = await _apiLogRepo.GetQueryableAsync();

        queryable = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(dataType), x => x.DataType == dataType)
            .WhereIf(isError.HasValue, x => x.IsError == isError)
            .WhereIf(dateFrom.HasValue, x => x.CreationTime >= dateFrom)
            .WhereIf(dateTo.HasValue, x => x.CreationTime <= dateTo!.Value.AddDays(1));

        var totalCount = await AsyncExecuter.CountAsync(queryable);

        var items = await AsyncExecuter.ToListAsync(
            queryable
                .OrderByDescending(x => x.CreationTime)
                .Skip((page - 1) * limit)
                .Take(limit)
        );

        var dtos = items.Select(x => new HlApiLogDto
        {
            Id = x.Id,
            HttpMethod = x.HttpMethod,
            RequestUrl = x.RequestUrl,
            ResponseStatusCode = x.ResponseStatusCode,
            DurationMs = x.DurationMs,
            IsError = x.IsError,
            ErrorMessage = x.ErrorMessage,
            DataType = x.DataType,
            CallerSource = x.CallerSource,
            CreationTime = x.CreationTime
        }).ToList();

        var paged = new HlPagedResponse<HlApiLogDto>
        {
            TotalRecords = totalCount,
            Page = page,
            Limit = limit,
            TotalPages = totalCount > 0 ? (int)Math.Ceiling((double)totalCount / limit) : 0,
            Data = dtos
        };

        return HlApiResult<HlPagedResponse<HlApiLogDto>>.Ok(paged);
    }

    public async Task<HlApiResult<int>> DeleteApiLogsAsync(string? dataType = null, bool? isError = null, DateTime? dateFrom = null, DateTime? dateTo = null)
    {
        await CheckPermissionAsync(MultiTenancyPermissions.AppHlApiLogs.Default, MultiTenancyPermissions.HostAppHlApiLogs.Default);

        var queryable = await _apiLogRepo.GetQueryableAsync();

        queryable = queryable
            .WhereIf(!string.IsNullOrWhiteSpace(dataType), x => x.DataType == dataType)
            .WhereIf(isError.HasValue, x => x.IsError == isError)
            .WhereIf(dateFrom.HasValue, x => x.CreationTime >= dateFrom)
            .WhereIf(dateTo.HasValue, x => x.CreationTime <= dateTo!.Value.AddDays(1));

        var items = await AsyncExecuter.ToListAsync(queryable);
        var count = items.Count;

        if (count > 0)
        {
            await _apiLogRepo.DeleteManyAsync(items, autoSave: true);
        }

        return HlApiResult<int>.Ok(count, $"Đã xóa {count} log");
    }

    #endregion
}
