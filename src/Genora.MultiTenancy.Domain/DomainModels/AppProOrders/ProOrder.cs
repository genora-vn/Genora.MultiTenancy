using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppProOrders;

[Table("AppProOrders")]
public class ProOrder : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string OrderCode { get; set; } = null!;

    /// <summary>Mã túi / thẻ golfer để nhận diện khách</summary>
    [Required]
    [StringLength(50)]
    public string BagTag { get; set; } = null!;

    /// <summary>
    /// Soft reference: có thể trỏ về AppCustomers (golf) hoặc AppSalonBeautyCustomers (salon).
    /// Không khai báo navigation/FK để Proshop dùng chung được nhiều domain khách hàng.
    /// Lookup customer trong service tùy theo entry point.
    /// </summary>
    public Guid? CustomerId { get; set; }

    [StringLength(150)]
    public string? CustomerName { get; set; }

    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    public decimal TotalAmount { get; set; }

    public ProServiceStatus ServiceStatus { get; set; } = ProServiceStatus.Created;

    public ProPaymentStatus PaymentStatus { get; set; } = ProPaymentStatus.Unpaid;

    public PaymentMethod? PaymentMethod { get; set; }

    public string? Note { get; set; }

    public string? InternalNote { get; set; }

    public ProCancelReason? CancelReason { get; set; }

    [StringLength(500)]
    public string? CancelNote { get; set; }

    public Guid? CancelledBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    public virtual ICollection<ProOrderItem> Items { get; set; } = new List<ProOrderItem>();

    protected ProOrder() { }

    public ProOrder(Guid id, string orderCode, string bagTag, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        OrderCode = orderCode;
        BagTag = bagTag;
    }
}
