using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class ProOrderDetailDto : ProOrderDto
{
    public string? CustomerTypeName { get; set; }
    public string? CustomerTypeColorCode { get; set; }

    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }

    public List<ProOrderItemDto> Items { get; set; } = new();
    public List<ProOrderActivityDto> Activities { get; set; } = new();
}
