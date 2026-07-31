using Genora.MultiTenancy.AppDtos.HoaLinh;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

public class HlApiClientService : IHlApiClientService
{
    private readonly IHttpClientFactory _httpFactory;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<HlApiClientService> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower
    };

    private class ProductGroupResolver : DefaultJsonTypeInfoResolver
    {
        public override JsonTypeInfo GetTypeInfo(Type type, JsonSerializerOptions options)
        {
            var info = base.GetTypeInfo(type, options);

            if (type == typeof(HlProductGroupDto))
            {
                var prop = info.Properties.FirstOrDefault(x => x.Name == "ProductCombo");
                if (prop != null)
                {
                    prop.Name = "productcombo";
                }
            }

            return info;
        }
    }


    public HlApiClientService(
        IHttpClientFactory httpFactory,
        ICurrentTenant currentTenant,
        ILogger<HlApiClientService> logger)
    {
        _httpFactory = httpFactory;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    #region Customers

    public async Task<HlApiResult<List<HlCustomerDto>>> GetCustomerByPhoneAsync(string phone)
    {
        var url = $"/api/get-customer-by-phone?phone={Uri.EscapeDataString(phone)}";
        return await GetAsync<List<HlCustomerDto>>(url, "Customer");
    }

    public async Task<HlApiResult<List<HlCustomerDto>>> GetCustomerDetailAsync(string phone)
    {
        var url = $"/api/Customers/{Uri.EscapeDataString(phone)}";
        return await GetAsync<List<HlCustomerDto>>(url, "Customer");
    }

    public async Task<HlApiResult<HlPagedResponse<HlCustomerDto>>> GetCustomersAsync(int page = 1, int limit = 50, string? search = null)
    {
        var url = $"/api/Customers?page={page}&limit={limit}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        return await GetAsync<HlPagedResponse<HlCustomerDto>>(url, "Customer");
    }

    #endregion

    #region Salemans

    public async Task<HlApiResult<HlPagedResponse<HlSalemanDto>>> GetSalemansAsync(int page = 1, int limit = 50)
    {
        var url = $"/api/Salemans?page={page}&limit={limit}";
        return await GetAsync<HlPagedResponse<HlSalemanDto>>(url, "Saleman");
    }

    public async Task<HlApiResult<List<HlSalemanDto>>> GetSalemanDetailAsync(string dsrCode)
    {
        var url = $"/api/Salemans/{Uri.EscapeDataString(dsrCode)}";
        return await GetAsync<List<HlSalemanDto>>(url, "Saleman");
    }

    #endregion

    #region Products

    public async Task<HlApiResult<HlPagedResponse<HlProductDto>>> GetProductsAsync(int page = 1, int limit = 50, string? search = null)
    {
        var url = $"/api/Products?page={page}&limit={limit}";
        if (!string.IsNullOrWhiteSpace(search))
            url += $"&search={Uri.EscapeDataString(search)}";
        return await GetAsync<HlPagedResponse<HlProductDto>>(url, "Product");
    }

    public async Task<HlApiResult<List<HlProductDto>>> GetProductDetailAsync(string productCode)
    {
        var url = $"/api/Products/{Uri.EscapeDataString(productCode)}";
        return await GetAsync<List<HlProductDto>>(url, "Product");
    }

    #endregion

    #region Orders

    public async Task<HlApiResult<HlPagedResponse<HlOrderDetailDto>>> GetOrdersAsync(int page = 1, int limit = 50, string? customerCode = null)
    {
        var url = $"/api/OrderDetails?page={page}&limit={limit}";
        if (!string.IsNullOrWhiteSpace(customerCode))
            url += $"&customer_code={Uri.EscapeDataString(customerCode)}";
        return await GetAsync<HlPagedResponse<HlOrderDetailDto>>(url, "Order");
    }

    public async Task<HlApiResult<List<HlOrderDetailDto>>> GetOrderDetailAsync(string orderNumber)
    {
        var url = $"/api/OrderDetails/{Uri.EscapeDataString(orderNumber)}";
        return await GetAsync<List<HlOrderDetailDto>>(url, "Order");
    }

    #endregion

    #region Campaigns

    public async Task<HlApiResult<HlPagedResponse<HlCampaignDto>>> GetCampaignsAsync(int page = 1, int limit = 50)
    {
        var url = $"/api/CustomerCampaigns?page={page}&limit={limit}";
        return await GetAsync<HlPagedResponse<HlCampaignDto>>(url, "Campaign");
    }

    public async Task<HlApiResult<List<HlCampaignDto>>> GetCampaignDetailAsync(string custCode)
    {
        var url = $"/api/CustomerCampaigns/{Uri.EscapeDataString(custCode)}";
        return await GetAsync<List<HlCampaignDto>>(url, "Campaign");
    }

    #endregion

    #region Brands

    public async Task<HlApiResult<HlPagedResponse<HlBrandDto>>> GetBrandsAsync(int page = 1, int limit = 50)
    {
        var url = $"/api/Brands?page={page}&limit={limit}";
        return await GetAsync<HlPagedResponse<HlBrandDto>>(url, "Brand");
    }

    public async Task<HlApiResult<List<HlBrandDto>>> GetBrandDetailAsync(string brandCode)
    {
        var url = $"/api/Brands/{Uri.EscapeDataString(brandCode)}";
        return await GetAsync<List<HlBrandDto>>(url, "Brand");
    }

    public async Task<HlApiResult<List<HlProductByBrandDto>>> GetProductsByBrandAsync(string brandCode)
    {
        var url = $"/api/get-products-by-brand?brand_code={Uri.EscapeDataString(brandCode)}";
        return await GetAsync<List<HlProductByBrandDto>>(url, "Product");
    }

    public async Task<HlApiResult<List<HlTopProductDto>>> GetTopProductsAsync(string customerCode)
    {
        var url = $"/api/TopCustomerProductsWithDetails/{Uri.EscapeDataString(customerCode)}";
        return await GetAsync<List<HlTopProductDto>>(url, "Product");
    }

    #endregion

    #region Product Groups

    public async Task<HlApiResult<HlPagedResponse<HlProductGroupDto>>> GetProductGroupsAsync(int page = 1, int limit = 50, short? isCombo = 0)
    {
        var url = $"/api/ProductGroup?page={page}&limit={limit}&is_combo={isCombo}";
        return await GetAsync<HlPagedResponse<HlProductGroupDto>>(url, "ProductGroup");
    }

    public async Task<HlApiResult<List<HlProductGroupDto>>> GetProductGroupDetailAsync(string code)
    {
        var url = $"/api/ProductGroup/{Uri.EscapeDataString(code)}";
        return await GetAsync<List<HlProductGroupDto>>(url, "ProductGroup");
    }

    #endregion

    #region Order Headers

    public async Task<HlApiResult<HlPagedResponse<HlOrderHeaderDto>>> GetOrderHeadersAsync(int page = 1, int limit = 50)
    {
        var url = $"/api/OrderHeaders?page={page}&limit={limit}";
        return await GetAsync<HlPagedResponse<HlOrderHeaderDto>>(url, "Order");
    }

    public async Task<HlApiResult<List<HlOrderHeaderDto>>> GetOrderHeaderDetailAsync(string orderNumber)
    {
        var url = $"/api/OrderHeaders/{Uri.EscapeDataString(orderNumber)}";
        return await GetAsync<List<HlOrderHeaderDto>>(url, "Order");
    }

    public async Task<HlApiResult<List<HlOrderHeaderDto>>> GetOrderHeaderZaloAsync(string customerCode, string? zaloOrderNumber = null)
    {
        var url = $"/api/get-order-header-zalo?customer_code={Uri.EscapeDataString(customerCode)}";
        if (!string.IsNullOrWhiteSpace(zaloOrderNumber))
            url += $"&zalo_order_number={Uri.EscapeDataString(zaloOrderNumber)}";
        return await GetAsync<List<HlOrderHeaderDto>>(url, "Order");
    }

    public async Task<HlApiResult<List<HlOrderDetailDto>>> GetOrderDetailZaloAsync(string customerCode, string zaloOrderNumber)
    {
        var url = $"/api/get-order-detail-zalo?customer_code={Uri.EscapeDataString(customerCode)}&zalo_order_number={Uri.EscapeDataString(zaloOrderNumber)}";
        return await GetAsync<List<HlOrderDetailDto>>(url, "Order");
    }

    #endregion

    #region Master Data

    public async Task<HlApiResult<HlPagedResponse<HlMasterOrderStatusDto>>> GetMasterOrderStatusAsync(int page = 1, int limit = 50)
    {
        var url = $"/api/MasterOrderStatus?page={page}&limit={limit}";
        return await GetAsync<HlPagedResponse<HlMasterOrderStatusDto>>(url, "MasterData");
    }

    public async Task<HlApiResult<List<HlProductComboDto>>> GetProductCombosAsync(int page = 1, int limit = 50)
    {
        var url = $"/api/ProductCombo?page={page}&limit={limit}";
        return await GetAsync<List<HlProductComboDto>>(url, "Product");
    }

    #endregion

    #region Private Helpers

    private async Task<HlApiResult<T>> GetAsync<T>(string relativeUrl, string dataType, string callerSource = "Admin")
    {
        var sw = Stopwatch.StartNew();
        var client = _httpFactory.CreateClient("HoaLinhDms");

        try
        {
            var response = await client.GetAsync(relativeUrl);
            sw.Stop();

            var responseBody = await response.Content.ReadAsStringAsync();

            _logger.LogInformation("HL API {Method} {Url} → {StatusCode} ({Duration}ms) [Type={DataType}, Source={Source}]",
                "GET", relativeUrl, (int)response.StatusCode, sw.ElapsedMilliseconds, dataType, callerSource);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("HL API Error: {StatusCode} {Reason} — {Url}", (int)response.StatusCode, response.ReasonPhrase, relativeUrl);
                return HlApiResult<T>.Fail($"API trả về lỗi: {(int)response.StatusCode}", response.ReasonPhrase);
            }

            var trimmed = responseBody.TrimStart();
            var data = DeserializeSmartResponse<T>(trimmed);

            if (data == null)
                return HlApiResult<T>.Fail("Không thể parse dữ liệu từ API Hoa Linh");

            return HlApiResult<T>.Ok(data);
        }
        catch (TaskCanceledException)
        {
            sw.Stop();
            _logger.LogWarning("HL API Timeout: {Url} ({Duration}ms)", relativeUrl, sw.ElapsedMilliseconds);
            return HlApiResult<T>.Fail("Timeout khi gọi API Hoa Linh");
        }
        catch (HttpRequestException ex)
        {
            sw.Stop();
            _logger.LogError(ex, "HL API Connection Error: {Url} ({Duration}ms)", relativeUrl, sw.ElapsedMilliseconds);
            return HlApiResult<T>.Fail("Lỗi kết nối tới API Hoa Linh", ex.Message);
        }
        catch (JsonException ex)
        {
            sw.Stop();
            _logger.LogError(ex, "HL API JSON Parse Error: {Url} ({Duration}ms)", relativeUrl, sw.ElapsedMilliseconds);
            return HlApiResult<T>.Fail("Lỗi parse dữ liệu từ API Hoa Linh", ex.Message);
        }
        catch (Exception ex)
        {
            sw.Stop();
            _logger.LogError(ex, "HL API Unexpected Error: {Url} ({Duration}ms)", relativeUrl, sw.ElapsedMilliseconds);
            return HlApiResult<T>.Fail("Lỗi không xác định", ex.Message);
        }
    }

    /// <summary>
    /// Smart deserialize: handle cả 2 format từ API Hoa Linh
    /// 1. Paged object: {"total_records":..., "data":[...]}
    /// 2. Array trực tiếp: [...]
    /// Nếu T là HlPagedResponse nhưng response là array → wrap thành paged
    /// </summary>
    private T? DeserializeSmartResponse<T>(string json)
    {
        if (json.Contains("\"productcombo\"", StringComparison.OrdinalIgnoreCase))
        {
            json = Regex.Replace(
                json,
                "\"productcombo\"\\s*:",
                "\"product_combo\":",
                RegexOptions.IgnoreCase);
        }
        var isArray = json.StartsWith("[");
        var targetType = typeof(T);

        // Nếu T là HlPagedResponse<X> mà response là array → wrap
        if (isArray && targetType.IsGenericType && targetType.GetGenericTypeDefinition() == typeof(HlPagedResponse<>))
        {
            var itemType = targetType.GetGenericArguments()[0];
            var listType = typeof(List<>).MakeGenericType(itemType);
            var list = JsonSerializer.Deserialize(json, listType, JsonOptions);

            if (list == null) return default;

            // Tạo HlPagedResponse<X> và gán data
            var paged = Activator.CreateInstance(targetType)!;
            var dataProperty = targetType.GetProperty("Data")!;
            var totalProperty = targetType.GetProperty("TotalRecords")!;
            var pageProperty = targetType.GetProperty("Page")!;
            var limitProperty = targetType.GetProperty("Limit")!;
            var totalPagesProperty = targetType.GetProperty("TotalPages")!;

            dataProperty.SetValue(paged, list);
            var count = ((System.Collections.ICollection)list).Count;
            totalProperty.SetValue(paged, count);
            pageProperty.SetValue(paged, 1);
            limitProperty.SetValue(paged, count);
            totalPagesProperty.SetValue(paged, 1);

            return (T)paged;
        }

        // Trường hợp bình thường: deserialize trực tiếp
        return JsonSerializer.Deserialize<T>(json, JsonOptions);
    }

    /// <summary>Truncate response body nếu quá 4000 ký tự để tránh bloat DB</summary>
    private static string? TruncateBody(string? body)
    {
        if (string.IsNullOrEmpty(body)) return body;
        return body.Length > 4000 ? body[..4000] + "...[truncated]" : body;
    }

    #endregion
}
