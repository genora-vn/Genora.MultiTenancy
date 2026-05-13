using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyBookingItemDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? ServiceCategoryName { get; set; }
    public Guid? StylistId { get; set; }
    public string? StylistName { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
}