using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyStylistLookupDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? RoleText { get; set; }
    public byte? Role { get; set; }
}