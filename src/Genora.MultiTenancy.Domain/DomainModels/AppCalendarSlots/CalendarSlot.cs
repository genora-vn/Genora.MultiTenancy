using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCalendarSlotPrices;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCalendarSlots;

[Table("AppCalendarSlots")]
public class CalendarSlot : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid GolfCourseId { get; set; }
    public virtual GolfCourse GolfCourse { get; set; } = null!;

    public DateTime ApplyDate { get; set; }      // date only
    public TimeSpan TimeFrom { get; set; }
    public TimeSpan TimeTo { get; set; }

    public PromotionType PromotionType { get; set; }

    public int MaxSlots { get; set; }

    [StringLength(500)]
    public string? InternalNote { get; set; }

    public bool IsActive { get; set; } = true;

    // Navigation
    public virtual ICollection<CalendarSlotPrice> Prices { get; set; } = new List<CalendarSlotPrice>();
    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    protected CalendarSlot() { }

    public CalendarSlot(Guid id, Guid golfCourseId, DateTime applyDate, TimeSpan from, TimeSpan to) : base(id)
    {
        GolfCourseId = golfCourseId;
        ApplyDate = applyDate;
        TimeFrom = from;
        TimeTo = to;
    }
}