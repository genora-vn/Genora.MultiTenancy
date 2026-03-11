using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public interface IAppHomePageConfigService : IApplicationService
{
    Task<PagedResultDto<HomePageWidgetListItemDto>> GetWidgetListAsync(GetHomePageWidgetListInput input);

    Task<HomePageWidgetDto> CreateWidgetAsync(CreateHomePageWidgetDto input);

    Task UpdateWidgetAsync(UpdateWidgetRequestDto input);

    Task UpdateWidgetOrderAsync(UpdateWidgetOrderDto input);

    Task<FeatureGridDto> GetFeatureGridAsync(Guid widgetId);

    Task UpdateFeatureGridAsync(Guid widgetId, UpdateFeatureGridDto input);

    Task<HomePageWidgetDto> GetWidgetAsync(Guid id);

    Task<HomePageWidgetDto> UpdateWidgetByIdAsync(Guid id, UpdateHomePageWidgetDto input);
}