using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class FnbOrderDetailDto : FnbOrderDto
{
    public List<FnbOrderItemDto> Items { get; set; } = new();
}