using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;

public class MiniAppSalonBeautyLocationDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string Address { get; set; } = null!;
    public string? Phone { get; set; }
    public string? OpenTime { get; set; }
    public string? CloseTime { get; set; }
    public string? ImageUrl { get; set; }
}
