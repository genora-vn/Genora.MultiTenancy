using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class GetProOrderHistoryInput : PagedAndSortedResultRequestDto
{
    public Guid OrderId { get; set; }
    public string? ActionType { get; set; }
}
