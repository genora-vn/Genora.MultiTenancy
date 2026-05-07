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

    public byte Type { get; set; }

    public int Point { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public virtual SalonBeautyCustomer? Customer { get; set; }
}
