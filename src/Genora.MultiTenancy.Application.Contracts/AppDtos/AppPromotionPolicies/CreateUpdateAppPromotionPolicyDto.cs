using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppPromotionPolicies;

public class CreateUpdateAppPromotionPolicyDto
{
    [Required]
    public Guid GolfCourseId { get; set; }

    [Required]
    public Guid PromotionTypeId { get; set; }

    [StringLength(255)]
    public string? PolicyTitle { get; set; }

    [Range(0, int.MaxValue)]
    public int? CancellationPolicyHours { get; set; }

    [Range(0, int.MaxValue)]
    public int? CancellationPolicyHoursWeekend { get; set; }

    public string? CancellationPolicyContent { get; set; }

    public List<PromotionPolicyGolfCourseDto> AvailableGolfCourses { get; set; } = new();
    public List<PromotionPolicyPromotionTypeDto> AvailablePromotionTypes { get; set; } = new();
}

public class PromotionPolicyGolfCourseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
}

public class PromotionPolicyPromotionTypeDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = default!;
    public string? ColorCode { get; set; }
}
