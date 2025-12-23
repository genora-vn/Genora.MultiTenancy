using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses;

public class CreateUpdateAppGolfCourseDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; }

    [Required]
    [StringLength(200)]
    public string Name { get; set; }

    [StringLength(255)]
    public string? Address { get; set; }

    [Required]
    [StringLength(100)]
    public string Province { get; set; }

    [Required]
    [StringLength(20)]
    public string Phone { get; set; }

    [StringLength(255)]
    public string? Website { get; set; }

    [StringLength(255)]
    public string? FanpageUrl { get; set; }

    [StringLength(500)]
    public string? ShortDescription { get; set; }

    [StringLength(500)]
    public string? AvatarUrl { get; set; }

    [StringLength(500)]
    public string? BannerUrl { get; set; }

    public string? CancellationPolicy { get; set; }
    public string? TermsAndConditions { get; set; }

    // dùng TimeSpan? vì OnModelCreating đang dùng Time
    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }

    public byte BookingStatus { get; set; } = 1;
    public bool IsActive { get; set; } = true;
    public string? FrameTimes { get; set; }

    public string? NumberHoles { get; set; }

    public string? Utilities { get; set; }
    public List<GolfCourseUtilityDto> AvailableUtilities { get; set; } = new List<GolfCourseUtilityDto>();
    public List<GolfCourseHoleDto> AvailableHoles { get; set; } = new List<GolfCourseHoleDto>();
    public List<GolfCourseSessionOfDayDto> AvailableSessionsOfDay { get; set; } = new List<GolfCourseSessionOfDayDto>();
}
public class GolfCourseUtilityDto
{
    public int UtilityId { get; set; }
    public string UtilityName { get; set; }
    public bool IsCheck { get; set; } = false;
}
public class GolfCourseHoleDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsCheck { get; set; } = false;
}
public class GolfCourseSessionOfDayDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsCheck { get; set; } = false;
}