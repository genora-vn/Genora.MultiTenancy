using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppHomePageConfigs;
public class EditWidgetModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateHomePageWidgetDto Widget { get; set; } = default!;

    private readonly IAppHomePageConfigService _service;

    public EditWidgetModalModel(IAppHomePageConfigService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetWidgetAsync(Id);
        Widget = ObjectMapper.Map<HomePageWidgetDto, UpdateHomePageWidgetDto>(dto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateWidgetByIdAsync(Id, Widget);
        return NoContent();
    }
}