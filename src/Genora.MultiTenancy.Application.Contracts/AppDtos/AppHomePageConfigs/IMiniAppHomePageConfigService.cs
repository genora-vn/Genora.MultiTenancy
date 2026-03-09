using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public interface IMiniAppHomePageConfigService : IApplicationService
{
    Task<MiniAppHomePageConfigDto> GetHomePageConfigAsync();
    Task<FeatureGridDto> GetFeatureGridAsync(Guid widgetId);
}