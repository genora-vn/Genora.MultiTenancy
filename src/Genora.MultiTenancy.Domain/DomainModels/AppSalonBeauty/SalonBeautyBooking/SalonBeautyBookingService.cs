using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyBookingServices")]
public class SalonBeautyBookingService : CreationAuditedEntity<Guid>
{
    public Guid BookingId { get; set; }

    public Guid ServiceId { get; set; }

    public decimal Price { get; set; }

    public int Duration { get; set; }

    public virtual SalonBeautyBooking? Booking { get; set; }
    public virtual SalonBeautyService? Service { get; set; }
}
