using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppEmails;

public class FnbOrderNewRequestEmailModelDto
{
    public string? OrderCode { get; set; }
    public string? BagTag { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public int ServiceStatus { get; set; }
    public string? ServiceStatusText { get; set; }
    public int PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public string? PaymentMethodText { get; set; }
    public string? Note { get; set; }

    // Bổ sung các trường hủy đơn
    public string? CancelReason { get; set; }
    public string? CancelNote { get; set; }

    public string? CreationTimeText { get; set; }
    public string? TotalAmountText { get; set; }

    public List<FnbOrderItemEmailItemDto> Items { get; set; } = new List<FnbOrderItemEmailItemDto>();
}

public class FnbOrderItemEmailItemDto
{
    public string? ItemName { get; set; }
    public string? PriceText { get; set; }
    public int Quantity { get; set; }
    public string? AmountText { get; set; }
    public string? Note { get; set; }
}