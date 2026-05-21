using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;

public class SalonBeautyLocationDto : EntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public TimeSpan OpenTime { get; set; }
    public TimeSpan CloseTime { get; set; }
    public string? OpenTimeText { get; set; }
    public string? CloseTimeText { get; set; }
    public int SlotDuration { get; set; }
    public int BufferTime { get; set; }
    public int MaxCapacityPerSlot { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; }
    public string? IsActiveText { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? IsShowOnAppText { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
}
