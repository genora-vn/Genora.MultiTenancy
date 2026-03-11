using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppHomePageConfigs;
public class CreateWidgetModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateHomePageWidgetDto Widget { get; set; } = new();

    private readonly IAppHomePageConfigService _service;

    public CreateWidgetModalModel(IAppHomePageConfigService service)
    {
        _service = service;
    }

    public void OnGet()
    {
        Widget = new CreateHomePageWidgetDto
        {
            ModuleKey = "Free",
            IsEnabled = true
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.CreateWidgetAsync(Widget);
        return NoContent();
    }
}