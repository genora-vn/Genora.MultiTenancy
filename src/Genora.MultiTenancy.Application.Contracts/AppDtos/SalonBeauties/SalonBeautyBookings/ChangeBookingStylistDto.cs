using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;

public class ChangeBookingStylistDto
{
    public Guid StylistId { get; set; }
    public string? Note { get; set; }
}
