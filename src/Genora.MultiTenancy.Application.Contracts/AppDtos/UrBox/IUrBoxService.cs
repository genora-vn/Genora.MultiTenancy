using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.UrBox;

/// <summary>
/// Service gọi API hệ thống UrBox (kho quà eVoucher).
/// - Tất cả API tra cứu: GET + query string.
/// - cartPayVoucher (đổi quà): POST + JSON body + header Signature (RSA-SHA256).
/// </summary>
public interface IUrBoxService
{
    /// <summary>Lấy danh sách thương hiệu (GET /4.0/gift/brand)</summary>
    Task<UrBoxResponse<UrBoxPagedData<UrBoxBrandDto>>> GetBrandsAsync(int? catId = null, int? perPage = null, int? pageNo = null);

    /// <summary>Lấy danh sách danh mục theo danh mục cha (GET /2.0/category/catbyparent)</summary>
    Task<UrBoxResponse<System.Collections.Generic.List<UrBoxCategoryDto>>> GetCategoriesAsync(int? parentId = null, string? lang = null);

    /// <summary>Lấy danh sách quà tặng (GET /4.0/gift/lists)</summary>
    Task<UrBoxResponse<UrBoxPagedData<UrBoxGiftItemDto>>> GetGiftListAsync(
        string? catId = null, string? brandId = null, string? field = null, string? lang = null,
        int? stock = null, string? title = null, int? perPage = null, int? pageNo = null);

    /// <summary>Lấy chi tiết 1 quà tặng (GET /4.0/gift/detail)</summary>
    Task<UrBoxResponse<UrBoxGiftDetailDto>> GetGiftDetailAsync(string giftId, string? lang = null);

    /// <summary>Lấy lịch sử đổi quà theo user (GET /2.0/cart/getlist)</summary>
    Task<UrBoxResponse<System.Collections.Generic.List<UrBoxCartDto>>> GetCartListByUserAsync(string siteUserId);

    /// <summary>Lấy chi tiết đơn theo transaction (GET /2.0/cart/getByTransaction)</summary>
    Task<UrBoxResponse<UrBoxCartByTransactionDto>> GetCartByTransactionAsync(string transactionId);

    /// <summary>
    /// Đổi quà eVoucher (POST /2.0/cart/cartPayVoucher — yêu cầu Signature).
    /// Lưu lịch sử vào AppHlGiftExchanges. Trả về response gốc từ UrBox.
    /// </summary>
    Task<UrBoxResponse<UrBoxRedeemData>> CreateOrderEvoucherAsync(UrBoxRedeemInput input);
}
