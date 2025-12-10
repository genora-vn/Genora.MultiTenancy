using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppBookingStatusHistories;

[Table("AppBookingStatusHistories")]
public class BookingStatusHistory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid BookingId { get; set; }
    public virtual Booking Booking { get; set; } = null!;

    public BookingStatus? OldStatus { get; set; }
    public BookingStatus NewStatus { get; set; }

    public DateTime ChangedAt { get; set; }
    [StringLength(100)]
    public string? ChangedBy { get; set; }

    protected BookingStatusHistory() { }

    public BookingStatusHistory(Guid id, Guid bookingId, BookingStatus newStatus) : base(id)
    {
        BookingId = bookingId;
        NewStatus = newStatus;
        ChangedAt = DateTime.UtcNow;
    }
}