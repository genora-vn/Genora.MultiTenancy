using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyServiceLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
}