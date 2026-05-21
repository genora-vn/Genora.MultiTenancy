using System;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

public class MiniAppSalonBeautyTimeSlotDto
{
    public Guid TimeSlotId { get; set; }
    public DateTime WorkDate { get; set; }
    public string StartTime { get; set; } = null!;
    public string EndTime { get; set; } = null!;
    public SalonBeautyTimeSlotStatus Status { get; set; }
    public bool IsShowOnApp { get; set; }
    public int BookedCount { get; set; }
    public int Capacity { get; set; }
}
