using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppNews;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppGolfCourses;

[Table("AppGolfCourses")]
public class GolfCourse : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string Code { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = null!;

    [StringLength(255)]
    public string? Address { get; set; }

    [StringLength(100)]
    public string? Province { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

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

    public TimeSpan? OpenTime { get; set; }
    public TimeSpan? CloseTime { get; set; }

    /// <summary>
    /// 0: Tạm ngừng, 1: Đang mở
    /// </summary>
    public byte BookingStatus { get; set; } = 1;

    [StringLength(50)]
    public string? FrameTimes { get; set; }

    [StringLength(50)]
    public string? NumberHoles { get; set; }

    [StringLength(20)]
    public string? Utilities { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<CalendarSlot> CalendarSlots { get; set; } = new List<CalendarSlot>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public virtual ICollection<News> News { get; set; } = new List<News>();

    protected GolfCourse() { }

    public GolfCourse(Guid id, string code, string name) : base(id)
    {
        Code = code;
        Name = name;
    }
}