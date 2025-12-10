using Genora.MultiTenancy.DomainModels.AppCustomerMemberships;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppMembershipTiers;

[Table("AppMembershipTiers")]
public class MembershipTier : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public decimal? MinTotalSpending { get; set; }
    public int? MinRounds { get; set; }

    public MembershipEvaluationPeriod EvaluationPeriod { get; set; } = MembershipEvaluationPeriod.Rolling12Months;

    public string? Benefits { get; set; }

    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; }

    // Navigation
    public virtual ICollection<CustomerMembership> CustomerMemberships { get; set; } = new List<CustomerMembership>();

    protected MembershipTier() { }

    public MembershipTier(Guid id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }
}