using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyCustomers")]
public class SalonBeautyCustomer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string CustomerCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(15)]
    public string? Phone { get; set; }

    [StringLength(255)]
    public string? Email { get; set; }

    public byte? Gender { get; set; }

    public DateTime? Birthday { get; set; }

    [StringLength(500)]
    public string? Avatar { get; set; }

    [StringLength(100)]
    public string? ZaloUserId { get; set; }

    public bool IsFollowOa { get; set; }

    public byte? Source { get; set; }

    public byte Status { get; set; } = 1;

    [StringLength(500)]
    public string? Note { get; set; }

    public virtual ICollection<SalonBeautyBooking> Bookings { get; set; } = new List<SalonBeautyBooking>();
    public virtual ICollection<SalonBeautyCustomerLoyaltyBalance> LoyaltyBalances { get; set; } = new List<SalonBeautyCustomerLoyaltyBalance>();
    public virtual ICollection<SalonBeautyCustomerLoyaltyTransaction> LoyaltyTransactions { get; set; } = new List<SalonBeautyCustomerLoyaltyTransaction>();
}
