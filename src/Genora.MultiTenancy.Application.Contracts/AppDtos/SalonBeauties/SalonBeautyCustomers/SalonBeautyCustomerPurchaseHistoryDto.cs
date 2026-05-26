using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

/// <summary>
/// Một dòng trong tab "Lịch sử mua hàng" — đổ từ AppProOrders match theo CustomerId hoặc CustomerPhone.
/// </summary>
public class SalonBeautyCustomerPurchaseHistoryDto
{
    public Guid Id { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public int ItemCount { get; set; }
    public string ItemsSummary { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string ServiceStatus { get; set; } = string.Empty;
    public string ServiceStatusText { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PaymentStatusText { get; set; } = string.Empty;
}
