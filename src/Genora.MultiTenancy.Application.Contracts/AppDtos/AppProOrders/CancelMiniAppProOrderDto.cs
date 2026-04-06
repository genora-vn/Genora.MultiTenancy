using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class CancelMiniAppProOrderDto
{
    public Guid? CustomerId { get; set; }

    [Required]
    [StringLength(100)]
    public string CancelReason { get; set; } = string.Empty;

    [StringLength(1000)]
    public string? CancelNote { get; set; }
}
