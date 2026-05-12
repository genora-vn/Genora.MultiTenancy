using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

public class SalonBeautyCustomerBookingHistoryDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string ServiceName { get; set; } = string.Empty;
    public string StylistName { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Status { get; set; } = string.Empty;
}
