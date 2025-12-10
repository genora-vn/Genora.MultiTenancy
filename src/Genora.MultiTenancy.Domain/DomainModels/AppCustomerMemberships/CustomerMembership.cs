using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCustomerMemberships;

[Table("AppCustomerMemberships")]
public class CustomerMembership : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid MembershipTierId { get; set; }
    public virtual MembershipTier MembershipTier { get; set; } = null!;

    public DateTime EffectiveFrom { get; set; }
    public DateTime? EffectiveTo { get; set; }

    public bool IsCurrent { get; set; }

    protected CustomerMembership() { }

    public CustomerMembership(Guid id, Guid customerId, Guid membershipTierId, DateTime effectiveFrom) : base(id)
    {
        CustomerId = customerId;
        MembershipTierId = membershipTierId;
        EffectiveFrom = effectiveFrom;
    }
}