namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO sản phẩm bán chạy theo khách hàng từ API Hoa Linh DMS
/// (GET /api/TopCustomerProductsWithDetails/{customerCode})
/// </summary>
public class HlTopProductDto
{
    public string? CustomerCode { get; set; }
    public string? ProductGroupCode { get; set; }
    public string? ProductGroupName { get; set; }
    public int? TotalQuantity { get; set; }
    public int? Rank { get; set; }
    public string? ImageAvatarUrl { get; set; }
    public decimal? ProductPrice { get; set; }
}
