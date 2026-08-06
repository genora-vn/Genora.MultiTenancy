using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppEmails;
public class ProOrderNewRequestEmailModelDto
{
    public string OrderCode { get; set; } = string.Empty;
    public string BagTag { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;

    public int ServiceStatus { get; set; }
    public string ServiceStatusText { get; set; } = string.Empty;

    public int PaymentStatus { get; set; }
    public string PaymentStatusText { get; set; } = string.Empty;
    public string PaymentMethodText { get; set; } = string.Empty;

    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public string? CancelNote { get; set; }

    public string CreationTimeText { get; set; } = string.Empty;
    public string TotalAmountText { get; set; } = string.Empty;

    public List<ProOrderItemEmailItemDto> Items { get; set; } = new List<ProOrderItemEmailItemDto>();
}

public class ProOrderItemEmailItemDto
{
    public string ItemName { get; set; } = string.Empty;
    public string PriceText { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public string AmountText { get; set; } = string.Empty;
    public string? Note { get; set; }
}