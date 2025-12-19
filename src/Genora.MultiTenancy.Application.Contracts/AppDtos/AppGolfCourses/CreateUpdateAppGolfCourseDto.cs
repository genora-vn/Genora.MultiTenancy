using System;
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
}