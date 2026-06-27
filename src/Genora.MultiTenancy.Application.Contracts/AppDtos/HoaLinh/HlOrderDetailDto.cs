namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// DTO chi tiết đơn hàng từ API Hoa Linh DMS (OrderDetails)
/// Mỗi record = 1 sản phẩm trong đơn hàng
/// </summary>
public class HlOrderDetailDto
{
    public string? DistributorCode { get; set; }
    public string? CustomerCode { get; set; }
    public string? OrderNumber { get; set; }
    public string? ProductSaleType { get; set; }
    public string? ProductCode { get; set; }
    public string? DistributorName { get; set; }
    public string? DsrCode { get; set; }
    public string? DsrName { get; set; }
    public string? CustomerName { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? ProductGroupCode { get; set; }
    public string? ProductGroupName { get; set; }
    public string? ProductName { get; set; }
    public string? ProductUnit { get; set; }
    public decimal? ProductPrice { get; set; }
    public int? Quantity { get; set; }
    public decimal? GrossValue { get; set; }
    public decimal? SchemeValue { get; set; }
    public decimal? NetValue { get; set; }
    public decimal? CreditNoteValue { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? OrderStatus { get; set; }
    public string? OrderDate { get; set; }
    public string? OrderTime { get; set; }
    public string? ProcessDate { get; set; }
    public string? ProcessTime { get; set; }
    public string? DeliveryDate { get; set; }
    public string? DeliveryTime { get; set; }
    public string? ZaloOrderNumber { get; set; }
    public int? RptMonth { get; set; }
    public int? RptYear { get; set; }
}
