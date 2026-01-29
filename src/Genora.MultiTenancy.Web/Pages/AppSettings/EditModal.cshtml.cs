using Genora.MultiTenancy.AppDtos.AppSettings;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppSettings;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppSettingDto AppSetting { get; set; }

    private readonly IAppSettingService _appSettingService;

    public EditModalModel(IAppSettingService appSettingService)
    {
        _appSettingService = appSettingService;
    }

    public async Task OnGetAsync()
    {
        var appSettingDto = await _appSettingService.GetAsync(Id);
        AppSetting = ObjectMapper.Map<AppSettingDto, CreateUpdateAppSettingDto>(appSettingDto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appSettingService.UpdateAsync(Id, AppSetting);
        return NoContent();
    }
}