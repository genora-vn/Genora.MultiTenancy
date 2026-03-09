using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class GetHomePageWidgetListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}