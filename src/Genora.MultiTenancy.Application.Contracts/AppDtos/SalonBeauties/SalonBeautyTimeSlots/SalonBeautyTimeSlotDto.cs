using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

public class SalonBeautyTimeSlotDto : EntityDto<Guid>
{
    public Guid LocationId { get; set; }
    public string? LocationName { get; set; }

    public Guid StylistId { get; set; }
    public string? StylistName { get; set; }
    public string? StylistAvatar { get; set; }

    public DateTime WorkDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }

    /// <summary>
    /// Số khách tối đa được phép book trong slot (không vượt quá Location.MaxCapacityPerSlot).
    /// </summary>
    public int Capacity { get; set; }

    /// <summary>
    /// Số khách đã book hiện tại. Khi BookedCount &gt;= Capacity → status sẽ tự chuyển Full.
    /// </summary>
    public int BookedCount { get; set; }

    /// <summary>
    /// Hiển thị "1/2" - số khách đã đặt / sức chứa.
    /// </summary>
    public string? CapacityText { get; set; }

    /// <summary>
    /// Manual override flag - admin can thiệp trạng thái slot, không cho recalculate đè.
    /// </summary>
    public bool IsManualOverride { get; set; }

    public byte Status { get; set; }
    public string? StatusText { get; set; }

    /// <summary>
    /// True nếu Status = PeakHour. Convenience flag cho FE.
    /// </summary>
    public bool IsPeakHour { get; set; }

    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
}
