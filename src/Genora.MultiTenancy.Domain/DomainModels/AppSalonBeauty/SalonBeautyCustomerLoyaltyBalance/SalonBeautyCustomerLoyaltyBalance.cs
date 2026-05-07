using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyCustomerLoyaltyBalances")]
public class SalonBeautyCustomerLoyaltyBalance : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CustomerId { get; set; }

    public int CurrentPoint { get; set; }

    public virtual SalonBeautyCustomer? Customer { get; set; }
}
