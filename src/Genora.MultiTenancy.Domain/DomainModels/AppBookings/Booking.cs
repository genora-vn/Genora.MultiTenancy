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

    public short? NumberHole { get; set; }

    [StringLength(20)]
    public string? Utility { get; set; }

    public bool IsExportInvoice { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }

    public BookingStatus Status { get; set; }

    public BookingSource Source { get; set; }

    [StringLength(200)]
    public string? CompanyName { get; set; }

    [StringLength(50)]
    public string? TaxCode { get; set; }

    [StringLength(500)]
    public string? CompanyAddress { get; set; }

    [StringLength(256)]
    public string? InvoiceEmail { get; set; }

    // Navigation
    public virtual ICollection<BookingPlayer> Players { get; set; } = new List<BookingPlayer>();
    public virtual ICollection<BookingStatusHistory> StatusHistories { get; set; } = new List<BookingStatusHistory>();

    protected Booking() { }

    public Booking(Guid id, string bookingCode, Guid customerId, Guid golfCourseId, Guid calendarSlotId, DateTime playDate, int numberOfGolfers, 
        decimal pricePerGolfer, decimal totalAmount, PaymentMethod? paymentMethod, BookingStatus bookingStatus, BookingSource source) : base(id)
    {
        BookingCode = bookingCode;
        CustomerId = customerId;
        GolfCourseId = golfCourseId;
        PlayDate = playDate;
        CalendarSlotId = calendarSlotId;
        NumberOfGolfers = numberOfGolfers;
        PricePerGolfer = pricePerGolfer;
        TotalAmount = totalAmount;
        PaymentMethod = paymentMethod;
        Status = bookingStatus;
        Source = source;
    }
}