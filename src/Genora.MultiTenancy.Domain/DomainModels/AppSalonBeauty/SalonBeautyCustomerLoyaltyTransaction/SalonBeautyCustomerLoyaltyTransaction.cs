using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyCustomerLoyaltyTransactions")]
public class SalonBeautyCustomerLoyaltyTransaction : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CustomerId { get; set; }

    /// <summary>LoyaltyTransactionType: Deposit=1, Earn=2, Redeem=3, Adjust=4, Refund=5.</summary>
    public byte Type { get; set; }

    /// <summary>Số điểm thay đổi: dương = cộng, âm = trừ.</summary>
    public int Point { get; set; }

    /// <summary>Số dư trước khi áp dụng giao dịch (audit).</summary>
    public int BalanceBefore { get; set; }

    /// <summary>Số dư sau khi áp dụng giao dịch (audit). BalanceAfter = BalanceBefore + Point.</summary>
    public int BalanceAfter { get; set; }

    /// <summary>Loại entity tham chiếu (Deposit=1, Booking=2, Manual=99).</summary>
    public byte ReferenceType { get; set; }

    /// <summary>Id của entity tham chiếu (nullable cho Manual).</summary>
    public Guid? ReferenceId { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public virtual SalonBeautyCustomer? Customer { get; set; }
}
