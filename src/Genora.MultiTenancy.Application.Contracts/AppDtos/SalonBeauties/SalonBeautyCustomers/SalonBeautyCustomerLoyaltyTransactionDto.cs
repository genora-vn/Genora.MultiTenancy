using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;

public class SalonBeautyCustomerLoyaltyTransactionDto
{
    public Guid Id { get; set; }
    public byte Type { get; set; }
    public string TypeText { get; set; } = string.Empty;
    public int Point { get; set; }
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}
