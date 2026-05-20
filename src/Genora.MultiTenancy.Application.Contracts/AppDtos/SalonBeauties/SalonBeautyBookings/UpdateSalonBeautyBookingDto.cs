using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class UpdateSalonBeautyBookingDto
{
    public Guid? LocationId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StylistId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    public decimal? Surcharge { get; set; }
    public decimal? Discount { get; set; }
    public List<CreateSalonBeautyBookingItemDto> Items { get; set; } = new();
}