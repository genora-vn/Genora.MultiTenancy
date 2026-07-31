using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.HoaLinh;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Interface gọi tất cả API Hoa Linh DMS
/// Mọi method đều log vào AppHlApiLogs
/// </summary>
public interface IHlApiClientService
{
    #region Customers

    /// <summary>
    /// Check khách hàng tồn tại trên DMS bằng SĐT (dùng cho auth Mini App)
    /// GET /api/get-customer-by-phone?phone={phone}
    /// </summary>
    Task<HlApiResult<List<HlCustomerDto>>> GetCustomerByPhoneAsync(string phone);

    /// <summary>
    /// Lấy chi tiết khách hàng qua SĐT
    /// GET /api/Customers/{phone}
    /// </summary>
    Task<HlApiResult<List<HlCustomerDto>>> GetCustomerDetailAsync(string phone);

    /// <summary>
    /// Lấy danh sách khách hàng (phân trang + tìm kiếm)
    /// GET /api/Customers?page={page}&limit={limit}&search={search}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlCustomerDto>>> GetCustomersAsync(int page = 1, int limit = 50, string? search = null);

    #endregion

    #region Salemans

    /// <summary>
    /// Lấy danh sách Sales (phân trang)
    /// GET /api/Salemans?page={page}&limit={limit}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlSalemanDto>>> GetSalemansAsync(int page = 1, int limit = 50);

    /// <summary>
    /// Lấy chi tiết Sale theo mã
    /// GET /api/Salemans/{dsrCode}
    /// </summary>
    Task<HlApiResult<List<HlSalemanDto>>> GetSalemanDetailAsync(string dsrCode);

    #endregion

    #region Products

    /// <summary>
    /// Lấy danh sách sản phẩm (phân trang + filter)
    /// GET /api/Products?page={page}&limit={limit}&search={search}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlProductDto>>> GetProductsAsync(int page = 1, int limit = 50, string? search = null);

    /// <summary>
    /// Lấy chi tiết sản phẩm theo mã
    /// GET /api/Products/{productCode}
    /// </summary>
    Task<HlApiResult<List<HlProductDto>>> GetProductDetailAsync(string productCode);

    /// <summary>
    /// Lấy danh sách sản phẩm combo (mỗi combo gồm nhiều dòng sản phẩm)
    /// GET /api/ProductCombo?page={page}&limit={limit}
    /// </summary>
    Task<HlApiResult<List<HlProductComboDto>>> GetProductCombosAsync(int page = 1, int limit = 50);

    #endregion

    #region Orders

    /// <summary>
    /// Lấy danh sách đơn hàng (phân trang)
    /// GET /api/OrderDetails?page={page}&limit={limit}&customer_code={customerCode}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlOrderDetailDto>>> GetOrdersAsync(int page = 1, int limit = 50, string? customerCode = null);

    /// <summary>
    /// Lấy chi tiết đơn hàng theo mã
    /// GET /api/OrderDetails/{orderNumber}
    /// </summary>
    Task<HlApiResult<List<HlOrderDetailDto>>> GetOrderDetailAsync(string orderNumber);

    #endregion

    #region Campaigns

    /// <summary>
    /// Lấy danh sách chiến dịch (phân trang)
    /// GET /api/CustomerCampaigns?page={page}&limit={limit}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlCampaignDto>>> GetCampaignsAsync(int page = 1, int limit = 50);

    /// <summary>
    /// Lấy chi tiết chiến dịch theo mã KH
    /// GET /api/CustomerCampaigns/{custCode}
    /// </summary>
    Task<HlApiResult<List<HlCampaignDto>>> GetCampaignDetailAsync(string custCode);

    #endregion

    #region Brands

    /// <summary>
    /// Lấy danh sách thương hiệu
    /// GET /api/Brands?page={page}&limit={limit}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlBrandDto>>> GetBrandsAsync(int page = 1, int limit = 50);

    /// <summary>
    /// Lấy chi tiết thương hiệu
    /// GET /api/Brands/{brandCode}
    /// </summary>
    Task<HlApiResult<List<HlBrandDto>>> GetBrandDetailAsync(string brandCode);

    /// <summary>
    /// Lấy sản phẩm theo brand
    /// GET /api/get-products-by-brand?brand_code={brandCode}
    /// </summary>
    Task<HlApiResult<List<HlProductByBrandDto>>> GetProductsByBrandAsync(string brandCode);

    /// <summary>
    /// Lấy danh sách sản phẩm bán chạy theo khách hàng (top mua nhiều)
    /// GET /api/TopCustomerProductsWithDetails/{customerCode}
    /// </summary>
    Task<HlApiResult<List<HlTopProductDto>>> GetTopProductsAsync(string customerCode);

    #endregion

    #region Product Groups

    /// <summary>
    /// Lấy danh sách nhóm sản phẩm
    /// GET /api/ProductGroup?page={page}&limit={limit}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlProductGroupDto>>> GetProductGroupsAsync(int page = 1, int limit = 500, short? isCombo = 0);

    /// <summary>
    /// Lấy chi tiết nhóm sản phẩm
    /// GET /api/ProductGroup/{code}
    /// </summary>
    Task<HlApiResult<List<HlProductGroupDto>>> GetProductGroupDetailAsync(string code);

    #endregion

    #region Order Headers

    /// <summary>
    /// Lấy danh sách order headers
    /// GET /api/OrderHeaders?page={page}&limit={limit}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlOrderHeaderDto>>> GetOrderHeadersAsync(int page = 1, int limit = 50);

    /// <summary>
    /// Lấy chi tiết order header
    /// GET /api/OrderHeaders/{orderNumber}
    /// </summary>
    Task<HlApiResult<List<HlOrderHeaderDto>>> GetOrderHeaderDetailAsync(string orderNumber);

    /// <summary>
    /// Lấy order header theo customer_code (bắt buộc) + zalo_order_number (không bắt buộc)
    /// GET /api/get-order-header-zalo?customer_code={}[&zalo_order_number={}]
    /// Không truyền zaloOrderNumber → lấy toàn bộ đơn theo mã khách hàng.
    /// </summary>
    Task<HlApiResult<List<HlOrderHeaderDto>>> GetOrderHeaderZaloAsync(string customerCode, string? zaloOrderNumber = null);

    /// <summary>
    /// Lấy order detail theo customer_code + zalo_order_number
    /// GET /api/get-order-detail-zalo?customer_code={}&zalo_order_number={}
    /// </summary>
    Task<HlApiResult<List<HlOrderDetailDto>>> GetOrderDetailZaloAsync(string customerCode, string zaloOrderNumber);

    #endregion

    #region Master Data

    /// <summary>
    /// Lấy danh sách trạng thái đơn hàng
    /// GET /api/MasterOrderStatus?page={page}&limit={limit}
    /// </summary>
    Task<HlApiResult<HlPagedResponse<HlMasterOrderStatusDto>>> GetMasterOrderStatusAsync(int page = 1, int limit = 50);

    #endregion
}
