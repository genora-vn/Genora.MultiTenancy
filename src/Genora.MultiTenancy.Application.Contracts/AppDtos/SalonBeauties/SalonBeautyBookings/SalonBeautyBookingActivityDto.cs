using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyBookingActivityDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Time { get; set; }
    public bool IsDanger { get; set; }
}