using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// AppService cho Admin Portal — wrap IHlApiClientService, expose qua ABP auto-proxy cho JS
/// </summary>
public interface IHlAdminAppService : IApplicationService
{
    // Products
    Task<HlApiResult<HlPagedResponse<HlProductDto>>> GetProductsAsync(int page, int limit, string? search = null);
    Task<HlApiResult<List<HlProductDto>>> GetProductDetailAsync(string productCode);

    // Brands
    Task<HlApiResult<HlPagedResponse<HlBrandDto>>> GetBrandsAsync(int page, int limit);
    Task<HlApiResult<List<HlBrandDto>>> GetBrandDetailAsync(string brandCode);
    Task<HlApiResult<List<HlProductByBrandDto>>> GetProductsByBrandAsync(string brandCode);

    // Product Groups
    Task<HlApiResult<HlPagedResponse<HlProductGroupDto>>> GetProductGroupsAsync(int page, int limit);
    Task<HlApiResult<List<HlProductGroupDto>>> GetProductGroupDetailAsync(string code);

    // Customers
    Task<HlApiResult<HlPagedResponse<HlCustomerDto>>> GetCustomersAsync(int page, int limit, string? search = null, int? source = null);
    Task<HlApiResult<List<HlCustomerDto>>> GetCustomerDetailAsync(string phone);
    Task<HlApiResult<List<HlCustomerDto>>> GetCustomerByPhoneAsync(string phone);

    // Salemans
    Task<HlApiResult<HlPagedResponse<HlSalemanDto>>> GetSalemansAsync(int page, int limit);
    Task<HlApiResult<List<HlSalemanDto>>> GetSalemanDetailAsync(string dsrCode);

    // Orders (from HL DMS)
    Task<HlApiResult<HlPagedResponse<HlOrderHeaderDto>>> GetOrderHeadersAsync(int page, int limit);
    Task<HlApiResult<List<HlOrderHeaderDto>>> GetOrderHeaderDetailAsync(string orderNumber);
    Task<HlApiResult<HlPagedResponse<HlOrderDetailDto>>> GetOrdersAsync(int page, int limit, string? customerCode = null);
    Task<HlApiResult<List<HlOrderDetailDto>>> GetOrderDetailAsync(string orderNumber);

    // Master Data
    Task<HlApiResult<HlPagedResponse<HlMasterOrderStatusDto>>> GetMasterOrderStatusAsync(int page, int limit);

    // Campaigns
    Task<HlApiResult<HlPagedResponse<HlCampaignDto>>> GetCampaignsAsync(int page, int limit);
    Task<HlApiResult<List<HlCampaignDto>>> GetCampaignDetailAsync(string custCode);

    // API Logs
    Task<HlApiResult<HlPagedResponse<HlApiLogDto>>> GetApiLogsAsync(int page, int limit, string? dataType = null, bool? isError = null, DateTime? dateFrom = null, DateTime? dateTo = null);
    Task<HlApiResult<int>> DeleteApiLogsAsync(string? dataType = null, bool? isError = null, DateTime? dateFrom = null, DateTime? dateTo = null);
}
