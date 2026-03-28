using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class GetFnbOrderHistoryInput : PagedAndSortedResultRequestDto
{
    public Guid OrderId { get; set; }
    public string? ActionType { get; set; }
}