using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class HlOrderCreatedEmailModelDto
{
    public string OrderCode { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerCode { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string BranchName { get; set; } = string.Empty;
    public string ReceiverName { get; set; } = string.Empty;
    public string ReceiverPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public string SubTotalText { get; set; } = string.Empty;
    public decimal DiscountAmount { get; set; }
    public decimal SystemDiscount { get; set; }
    public string DiscountText { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public string GrandTotalText { get; set; } = string.Empty;
    public string CreationTimeText { get; set; } = string.Empty;
    public string OrderStatusText { get; set; } = string.Empty;
    public string PaymentStatusText { get; set; } = string.Empty;
    public string PaymentMethodText { get; set; } = string.Empty;
    public string DeliveryStatusText { get; set; } = string.Empty;
    public string ShippingAddress { get; set; } = string.Empty;
    public string Note { get; set; } = string.Empty;
    public int TotalItemsCount { get; set; }
    public List<HlOrderItemEmailDto> Items { get; set; } = new();
}

public class HlOrderItemEmailDto
{
    public string BrandName { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string ProductName { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string UnitPriceText { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Amount { get; set; }
    public string TotalPriceText { get; set; } = string.Empty;
    public string ProductUnit { get; set; } = string.Empty;
}
