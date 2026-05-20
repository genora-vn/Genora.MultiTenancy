using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

/// <summary>
/// 1 dòng/ stylist trong danh sách Lịch làm việc (group theo stylist).
/// FromDate / ToDate là ngày đầu - cuối stylist được cấu hình.
/// FromTime / ToTime là giờ sớm nhất - giờ muộn nhất trong ngày.
/// </summary>
public class SalonBeautyTimeSlotGroupedDto
{
    public Guid StylistId { get; set; }
    public string StylistName { get; set; } = null!;
    public string? StylistAvatar { get; set; }

    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }

    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    public TimeSpan? FromTime { get; set; }
    public TimeSpan? ToTime { get; set; }

    public bool IsActive { get; set; }
    public bool IsShowOnApp { get; set; }

    public int SlotCount { get; set; }
}

public class GetSalonBeautyTimeSlotListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? StylistId { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public byte? Status { get; set; }
    public bool? IsShowOnApp { get; set; }
}

public class GetSalonBeautyTimeSlotCalendarInput
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public Guid? LocationId { get; set; }
    public Guid? StylistId { get; set; }
    public byte? Status { get; set; }
}
