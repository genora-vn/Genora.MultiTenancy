using System;
using System.IO;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class SettingsModel : AbpPageModel
{
    private readonly IHl25AppConfigAppService _appConfigService;

    [BindProperty]
    public Hl25SettingsViewModel Input { get; set; } = new();

    public SettingsModel(IHl25AppConfigAppService appConfigService)
    {
        _appConfigService = appConfigService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _appConfigService.GetAsync();
        Input = new Hl25SettingsViewModel
        {
            ProgramName = dto.ProgramName,
            RulesHtml = dto.RulesHtml,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Scope = dto.Scope,
            OrganizerName = dto.OrganizerName,
            IsActive = dto.IsActive
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appConfigService.UpdateAsync(new CreateUpdateHl25AppConfigDto
        {
            ProgramName = Input.ProgramName,
            RulesHtml = Input.RulesHtml,
            StartTime = Input.StartTime,
            EndTime = Input.EndTime,
            Scope = Input.Scope,
            OrganizerName = Input.OrganizerName,
            IsActive = Input.IsActive
        });

        return RedirectToPage();
    }

    public class Hl25SettingsViewModel
    {
        public string? ProgramName { get; set; }
        public string? RulesHtml { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Scope { get; set; }
        public string? OrganizerName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
