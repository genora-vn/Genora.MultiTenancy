using Genora.MultiTenancy.DomainModels.AppBookingPlayers;
using Genora.MultiTenancy.DomainModels.AppBookingStatusHistories;
using Genora.MultiTenancy.DomainModels.AppCalendarSlots;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppBookings;

[Table("AppBookings")]
public class Booking : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string BookingCode { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public virtual Customer Customer { get; set; } = null!;

    public Guid GolfCourseId { get; set; }
    public virtual GolfCourse GolfCourse { get; set; } = null!;

    public DateTime PlayDate { get; set; } // date only

    public Guid? CalendarSlotId { get; set; }
    public virtual CalendarSlot? CalendarSlot { get; set; }

    public int NumberOfGolfers { get; set; }

    public decimal? PricePerGolfer { get; set; }
    public decimal TotalAmount { get; set; }

    [StringLength(50)]
    public string? PaymentMethod { get; set; }

    public BookingStatus Status { get; set; }

    [StringLength(50)]
    public string? Source { get; set; }

    // Navigation
    public virtual ICollection<BookingPlayer> Players { get; set; } = new List<BookingPlayer>();
    public virtual ICollection<BookingStatusHistory> StatusHistories { get; set; } = new List<BookingStatusHistory>();

    protected Booking() { }

    public Booking(Guid id, string bookingCode, Guid customerId, Guid golfCourseId, DateTime playDate) : base(id)
    {
        BookingCode = bookingCode;
        CustomerId = customerId;
        GolfCourseId = golfCourseId;
        PlayDate = playDate;
    }
}