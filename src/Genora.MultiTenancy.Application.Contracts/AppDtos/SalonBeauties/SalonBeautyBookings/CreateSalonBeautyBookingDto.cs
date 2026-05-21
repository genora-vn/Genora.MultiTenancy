using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class CreateSalonBeautyBookingDto
{
    public Guid? LocationId { get; set; }
    public Guid CustomerId { get; set; }
    public Guid StylistId { get; set; }

    /// <summary>
    /// ID time slot đã cấu hình. Khi truyền, BE sẽ lấy WorkDate / StartTime / EndTime từ slot
    /// và bỏ qua BookingDate / StartTime / EndTime trong input.
    /// </summary>
    public Guid? TimeSlotId { get; set; }

    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    public decimal? Surcharge { get; set; }
    public decimal? Discount { get; set; }
    public List<CreateSalonBeautyBookingItemDto> Items { get; set; } = new();
}