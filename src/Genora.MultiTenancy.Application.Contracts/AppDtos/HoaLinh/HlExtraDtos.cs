namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO thương hiệu (Danh mục sản phẩm) từ API Hoa Linh DMS
/// </summary>
public class HlBrandDto
{
    public int? BrandCode { get; set; }
    public string? BrandName { get; set; }
    public bool? IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public int? NoOfProduct { get; set; }
    public int? Seq { get; set; }
}

/// <summary>
/// DTO sản phẩm theo brand (get-products-by-brand) — có thêm description, instruction
/// </summary>
public class HlProductByBrandDto
{
    public string? ProductGroupCode { get; set; }
    public string? ProductGroupName { get; set; }
    public string? Description { get; set; }
    public string? Instruction { get; set; }
    public int? BrandCode { get; set; }
    public string? BrandName { get; set; }
    public string? ProductUnit { get; set; }
    public decimal? ProductPrice { get; set; }
    public bool? IsActive { get; set; }
    public string? ImageAvatarUrl { get; set; }
    public string? ImageUrl { get; set; }
    public double? DiscPercent { get; set; }
    public bool? IsCombo { get; set; }
}

/// <summary>
/// DTO Order Header từ API Hoa Linh DMS
/// </summary>
public class HlOrderHeaderDto
{
    public string? DistributorCode { get; set; }
    public string? CustomerCode { get; set; }
    public string? OrderNumber { get; set; }
    public string? DistributorName { get; set; }
    public string? DsrCode { get; set; }
    public string? DsrName { get; set; }
    public string? CustomerName { get; set; }
    public string? DeliveryAddress { get; set; }
    public decimal? GrossValue { get; set; }
    public decimal? TotalAmount { get; set; }
    public int? OrderStatusCode { get; set; }
    public string? OrderStatus { get; set; }
    public string? OrderDate { get; set; }
    public string? OrderTime { get; set; }
    public string? ZaloOrderNumber { get; set; }
    public int? RptMonth { get; set; }
    public int? RptYear { get; set; }
}

/// <summary>
/// DTO Master Order Status từ API Hoa Linh DMS
/// </summary>
public class HlMasterOrderStatusDto
{
    public int? StatusId { get; set; }
    public string? StatusName { get; set; }
    public string? Description { get; set; }
}

/// <summary>
/// DTO Product Group từ API Hoa Linh DMS
/// </summary>
public class HlProductGroupDto
{
    public string? ProductGroupCode { get; set; }
    public string? ProductGroupName { get; set; }
    public string? Description { get; set; }
    public string? Instruction { get; set; }
    public int? BrandCode { get; set; }
    public string? BrandName { get; set; }
    public string? ProductUnit { get; set; }
    public decimal? ProductPrice { get; set; }
    public bool? IsActive { get; set; }
    public string? ImageAvatarUrl { get; set; }
    public string? ImageUrl { get; set; }
    public double? DiscPercent { get; set; }
    public bool? IsCombo { get; set; }
}

/// <summary>
/// DTO hình thức thanh toán khả dụng cho Mini App hiển thị.
/// Đọc từ ABP Setting toggle (Thanh toán tại quầy / Chuyển khoản).
/// </summary>
public class HlPaymentMethodDto
{
    /// <summary>Mã hình thức: COUNTER | BANK_TRANSFER</summary>
    public string Code { get; set; } = string.Empty;

    /// <summary>Tên hiển thị tiếng Việt</summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>Có đang bật hay không (luôn true trong danh sách trả về)</summary>
    public bool IsEnabled { get; set; }
}
