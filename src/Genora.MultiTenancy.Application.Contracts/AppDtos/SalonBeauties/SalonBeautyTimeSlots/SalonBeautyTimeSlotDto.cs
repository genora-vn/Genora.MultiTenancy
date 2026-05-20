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

    public byte Status { get; set; }
    public string? StatusText { get; set; }

    public bool IsShowOnApp { get; set; }
    public string? Note { get; set; }
}
