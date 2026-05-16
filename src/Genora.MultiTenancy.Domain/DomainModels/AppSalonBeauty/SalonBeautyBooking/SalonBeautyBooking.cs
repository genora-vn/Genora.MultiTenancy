using System.ComponentModel.DataAnnotations;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;
using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyBookings", Schema = "Salon")]
public class SalonBeautyBooking : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string BookingCode { get; set; } = null!;

    public Guid CustomerId { get; set; }

    public Guid ServiceId { get; set; }

    public Guid StylistId { get; set; }

    public DateTime BookingDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public decimal TotalAmount { get; set; }

    public SalonBeautyBookingStatus Status { get; set; } = SalonBeautyBookingStatus.New;

    public SalonBeautyPaymentStatus PaymentStatus { get; set; } = SalonBeautyPaymentStatus.Unpaid;

    public SalonBeautyPaymentMethod? PaymentMethod { get; set; }

    public SalonBeautyCheckinStatus CheckinStatus { get; set; } = SalonBeautyCheckinStatus.NotCheckedIn;

    public DateTime? CheckinTime { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public SalonBeautyCancelReason? CancelReason { get; set; }

    [StringLength(500)]
    public string? CancelNote { get; set; }

    public virtual SalonBeautyCustomer? Customer { get; set; }
    public virtual SalonBeautyService? Service { get; set; }
    public virtual SalonBeautyStylist? Stylist { get; set; }
    public virtual ICollection<SalonBeautyBookingService> BookingServices { get; set; } = new List<SalonBeautyBookingService>();

    protected SalonBeautyBooking()
    {
    }

    public SalonBeautyBooking(
        Guid id,
        string bookingCode,
        Guid customerId,
        Guid serviceId,
        Guid stylistId,
        DateTime bookingDate,
        TimeSpan startTime,
        TimeSpan endTime,
        decimal totalAmount,
        SalonBeautyBookingStatus status,
        SalonBeautyPaymentStatus paymentStatus,
        SalonBeautyCheckinStatus checkinStatus,
        string? note,
        Guid? tenantId = null) : base(id)
    {
        BookingCode = bookingCode;
        CustomerId = customerId;
        ServiceId = serviceId;
        StylistId = stylistId;
        BookingDate = bookingDate.Date;
        StartTime = startTime;
        EndTime = endTime;
        TotalAmount = totalAmount;
        Status = status;
        PaymentStatus = paymentStatus;
        CheckinStatus = checkinStatus;
        Note = note;
        TenantId = tenantId;
    }
}
