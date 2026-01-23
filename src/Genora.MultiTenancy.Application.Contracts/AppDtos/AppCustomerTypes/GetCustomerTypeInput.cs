using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCustomerTypes;

public class GetCustomerTypeInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}