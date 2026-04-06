using System;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class ProOrderActivityDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime Time { get; set; }
    public bool IsDanger { get; set; }
}
