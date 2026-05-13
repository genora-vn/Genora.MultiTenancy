using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class CreateSalonBeautyBookingItemDto
{
    public Guid ServiceId { get; set; }
    public Guid? StylistId { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
}