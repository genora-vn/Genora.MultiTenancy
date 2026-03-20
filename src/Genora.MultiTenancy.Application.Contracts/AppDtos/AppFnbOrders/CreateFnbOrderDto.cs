using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class CreateFnbOrderDto
{
    public Guid? CustomerId { get; set; }

    [StringLength(150)]
    public string? CustomerName { get; set; }

    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    [Required]
    [StringLength(50)]
    public string BagTag { get; set; } = null!;

    [Required]
    public List<CreateFnbOrderItemDto> Items { get; set; } = new();

    public string? Note { get; set; }

    public string? InternalNote { get; set; }

    public PaymentMethod? PaymentMethod { get; set; }
}