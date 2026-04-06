using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class ProOrderDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public string OrderCode { get; set; } = null!;
    public string BagTag { get; set; } = null!;
    public Guid? CustomerId { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public decimal TotalAmount { get; set; }
    public ProServiceStatus ServiceStatus { get; set; }
    public ProPaymentStatus PaymentStatus { get; set; }
    public PaymentMethod? PaymentMethod { get; set; }
    public string? Note { get; set; }
    public string? InternalNote { get; set; }
    public ProCancelReason? CancelReason { get; set; }
    public string? CancelNote { get; set; }
}
