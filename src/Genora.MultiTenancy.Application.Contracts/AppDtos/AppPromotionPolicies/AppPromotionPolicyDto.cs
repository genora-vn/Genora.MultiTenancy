using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppPromotionPolicies;

public class AppPromotionPolicyDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }
    public Guid GolfCourseId { get; set; }
    public Guid PromotionTypeId { get; set; }
    public string? PolicyTitle { get; set; }
    public int? CancellationPolicyHours { get; set; }
    public int? CancellationPolicyHoursWeekend { get; set; }
    public string? CancellationPolicyContent { get; set; }

    public string? GolfCourseName { get; set; }
    public string? PromotionTypeName { get; set; }
    public string? PromotionTypeColor { get; set; }
}
