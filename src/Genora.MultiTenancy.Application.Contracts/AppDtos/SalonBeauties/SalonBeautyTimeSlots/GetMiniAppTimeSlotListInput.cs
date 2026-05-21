using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

public class GetMiniAppTimeSlotListInput
{
    public Guid? LocationId { get; set; }
    public DateTime? Date { get; set; }
    public Guid? StylistId { get; set; }
}
