using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppHomePageConfigs;
public class FeatureGridModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid WidgetId { get; set; }

    [BindProperty]
    public UpdateFeatureGridDto Model { get; set; } = new();

    private readonly IAppHomePageConfigService _service;

    public FeatureGridModalModel(IAppHomePageConfigService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetFeatureGridAsync(WidgetId);
        Model = ObjectMapper.Map<FeatureGridDto, UpdateFeatureGridDto>(dto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateFeatureGridAsync(WidgetId, Model);
        return NoContent();
    }
}