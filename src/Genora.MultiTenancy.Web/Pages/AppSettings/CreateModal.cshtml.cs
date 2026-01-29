using Genora.MultiTenancy.AppDtos.AppSettings;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppSettings;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppSettingDto AppSetting { get; set; }

    private readonly IAppSettingService _appSettingService;

    public CreateModalModel(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    public void OnGet()
    {
        AppSetting = new CreateUpdateAppSettingDto();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appSettingService.CreateAsync(AppSetting);
        return NoContent();
    }
}