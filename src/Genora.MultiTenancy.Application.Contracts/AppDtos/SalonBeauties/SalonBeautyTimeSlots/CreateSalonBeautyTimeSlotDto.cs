using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

/// <summary>
/// Tạo mới lịch làm việc cho 1 stylist tại 1 cơ sở, theo dải ngày + danh sách khung giờ.
/// Backend sẽ generate ra nhiều SalonBeautyTimeSlot rows.
/// </summary>
public class CreateSalonBeautyTimeSlotDto
{
    [Required]
    public Guid LocationId { get; set; }

    [Required]
    public Guid StylistId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    /// <summary>
    /// Danh sách khung giờ trong ngày, ví dụ: 08:00-12:00, 13:00-19:00.
    /// Tối thiểu 1 khung giờ.
    /// </summary>
    [Required]
    [MinLength(1)]
    public List<TimeRangeDto> Ranges { get; set; } = new();

    /// <summary>
    /// Bitmask weekdays (Sun=1, Mon=2, ..., Sat=64). 0 hoặc 127 = áp dụng tất cả các ngày.
    /// </summary>
    [Range(0, 127)]
    public int WeekdayMask { get; set; }

    public bool IsShowOnApp { get; set; } = true;

    public byte Status { get; set; } = 1;

    [StringLength(500)]
    public string? Note { get; set; }
}

public class TimeRangeDto
{
    [Required]
    public TimeSpan StartTime { get; set; }

    [Required]
    public TimeSpan EndTime { get; set; }
}
