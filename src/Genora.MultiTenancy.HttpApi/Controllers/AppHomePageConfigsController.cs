using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Controllers;

[ApiController]
[IgnoreAntiforgeryToken]
[Route("api/app/app-home-page-config")]
public class AppHomePageConfigsController : MultiTenancyController
{
    private readonly IAppHomePageConfigService _service;

    public AppHomePageConfigsController(IAppHomePageConfigService service)
    {
        _service = service;
    }

    [HttpPost("update-widget")]
    public Task UpdateWidgetAsync([FromBody] UpdateWidgetRequestDto input)
    {
        return _service.UpdateWidgetAsync(input);
    }

    [HttpPost("update-widget-order")]
    public Task UpdateWidgetOrderAsync([FromBody] UpdateWidgetOrderDto input)
    {
        return _service.UpdateWidgetOrderAsync(input);
    }

    [HttpPost("{widgetId}/update-feature-grid")]
    public Task UpdateFeatureGridAsync(Guid widgetId, [FromBody] UpdateFeatureGridDto input)
    {
        return _service.UpdateFeatureGridAsync(widgetId, input);
    }

    [HttpPost("{id}/update-widget-by-id")]
    public Task<HomePageWidgetDto> UpdateWidgetByIdAsync(Guid id, [FromBody] UpdateHomePageWidgetDto input)
    {
        return _service.UpdateWidgetByIdAsync(id, input);
    }

    [HttpPost("{id}")]
    public Task<HomePageWidgetDto> UpdateAsync(Guid id, [FromBody] UpdateHomePageWidgetDto input)
    {
        return _service.UpdateWidgetByIdAsync(id, input);
    }
}
