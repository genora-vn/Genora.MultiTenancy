using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyCustomerLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string? Code { get; set; }
}