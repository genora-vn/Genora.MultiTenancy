using System;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class FnbOrderActivityDto
{
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public DateTime Time { get; set; }
    public bool IsDanger { get; set; }
}