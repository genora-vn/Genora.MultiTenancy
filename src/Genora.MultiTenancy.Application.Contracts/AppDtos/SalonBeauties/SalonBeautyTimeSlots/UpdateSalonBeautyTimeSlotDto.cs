using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

/// <summary>
/// Cập nhật lịch làm việc của 1 stylist (toàn bộ schedule).
/// Replace toàn bộ slot trong dải ngày FromDate..ToDate cho stylist tại location.
/// </summary>
public class UpdateSalonBeautyTimeSlotDto
{
    [Required]
    public Guid LocationId { get; set; }

    [Required]
    public Guid StylistId { get; set; }

    [Required]
    public DateTime FromDate { get; set; }

    [Required]
    public DateTime ToDate { get; set; }

    [Required]
    [MinLength(1)]
    public List<TimeRangeDto> Ranges { get; set; } = new();

    [Range(0, 127)]
    public int WeekdayMask { get; set; }

    public bool IsShowOnApp { get; set; } = true;

    public byte Status { get; set; } = 1;

    [StringLength(500)]
    public string? Note { get; set; }
}

/// <summary>
/// Cập nhật trạng thái 1 time slot khi click trên Calendar (OFF/ON/FULL/PEAK).
/// </summary>
public class UpdateSalonBeautyTimeSlotStatusDto
{
    [Required]
    [Range(0, 3)]
    public byte Status { get; set; }
}
