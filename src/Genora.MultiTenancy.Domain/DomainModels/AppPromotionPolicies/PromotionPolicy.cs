using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppPromotionPolicies;

[Table("AppPromotionPolicies")]
public class PromotionPolicy : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid GolfCourseId { get; set; }

    public Guid PromotionTypeId { get; set; }

    [StringLength(255)]
    public string? PolicyTitle { get; set; }

    public int? CancellationPolicyHours { get; set; }

    public int? CancellationPolicyHoursWeekend { get; set; }

    public string? CancellationPolicyContent { get; set; }

    public virtual GolfCourse? GolfCourse { get; set; }
    public virtual PromotionType? PromotionType { get; set; }

    protected PromotionPolicy() { }

    public PromotionPolicy(Guid id, Guid golfCourseId, Guid promotionTypeId) : base(id)
    {
        GolfCourseId = golfCourseId;
        PromotionTypeId = promotionTypeId;
    }
}
