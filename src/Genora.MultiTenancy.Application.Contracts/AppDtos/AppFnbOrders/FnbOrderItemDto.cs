using System;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class FnbOrderItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ItemId { get; set; }
    public string ItemName { get; set; } = null!;
    public decimal Price { get; set; }
    public int Quantity { get; set; }
    public string? ImageUrl { get; set; }
    public string? Note { get; set; }
    public decimal LineTotal => Price * Quantity;
}