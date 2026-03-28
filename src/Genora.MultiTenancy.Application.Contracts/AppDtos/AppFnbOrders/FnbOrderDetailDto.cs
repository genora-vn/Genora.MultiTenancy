using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class FnbOrderDetailDto : FnbOrderDto
{
    public Guid Id { get; set; }
    public Guid? TenantId { get; set; }
    public string OrderCode { get; set; } = default!;
    public string BagTag { get; set; } = default!;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneMasked { get; set; }

    public string? CustomerTypeName { get; set; }
    public string? CustomerTypeColorCode { get; set; }

    public decimal TotalAmount { get; set; }
    public FnbServiceStatus ServiceStatus { get; set; }
    public FnbPaymentStatus PaymentStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }

    public string? Note { get; set; }
    public string? InternalNote { get; set; }
    public FnbCancelReason? CancelReason { get; set; }
    public string? CancelNote { get; set; }
    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    public DateTime CreationTime { get; set; }
    public Guid? CreatorId { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public Guid? LastModifierId { get; set; }

    public List<FnbOrderItemDto> Items { get; set; } = new();
    public List<FnbOrderActivityDto> Activities { get; set; } = new();
}