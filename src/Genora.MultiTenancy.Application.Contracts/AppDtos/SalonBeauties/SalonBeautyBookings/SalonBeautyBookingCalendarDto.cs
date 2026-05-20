using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyBookingCalendarDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = null!;
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? CustomerPhone { get; set; }
    public Guid StylistId { get; set; }
    public string StylistName { get; set; } = null!;
    public string? ServiceName { get; set; }
    public int ServiceCount { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = null!;
    public string StatusText { get; set; } = null!;
    public string StatusColor { get; set; } = null!;
    public decimal TotalAmount { get; set; }
}