using System;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses;

public class GolfCoursePromotionTypeDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string? ColorCode { get; set; }
    public bool IsCheck { get; set; }
}