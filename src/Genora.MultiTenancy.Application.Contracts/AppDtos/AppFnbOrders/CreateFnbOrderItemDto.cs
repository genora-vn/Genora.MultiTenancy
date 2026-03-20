using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class CreateFnbOrderItemDto
{
    [Required]
    public Guid ItemId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; } = 1;

    public string? Note { get; set; }
}