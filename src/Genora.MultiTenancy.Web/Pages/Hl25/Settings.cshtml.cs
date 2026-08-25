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

    [BindProperty]
    public IFormFile? LogoFile { get; set; }

    [BindProperty]
    public IFormFile? BannerFile { get; set; }

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
            LogoUrl = dto.LogoUrl,
            BannerUrl = dto.BannerUrl,
            TvcUrl = dto.TvcUrl,
            TvcHtml = dto.TvcHtml,
            RulesHtml = dto.RulesHtml,
            GamePlayHtml = dto.GamePlayHtml,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Scope = dto.Scope,
            OrganizerName = dto.OrganizerName,
            IsActive = dto.IsActive
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Upload logo/banner nếu có file mới (giới hạn 5MB được kiểm tra trong AppService).
        if (LogoFile != null && LogoFile.Length > 0)
        {
            Input.LogoUrl = await UploadAsync(LogoFile, "logo");
        }

        if (BannerFile != null && BannerFile.Length > 0)
        {
            Input.BannerUrl = await UploadAsync(BannerFile, "banner");
        }

        await _appConfigService.UpdateAsync(new CreateUpdateHl25AppConfigDto
        {
            ProgramName = Input.ProgramName,
            LogoUrl = Input.LogoUrl,
            BannerUrl = Input.BannerUrl,
            TvcUrl = Input.TvcUrl,
            TvcHtml = Input.TvcHtml,
            RulesHtml = Input.RulesHtml,
            GamePlayHtml = Input.GamePlayHtml,
            StartTime = Input.StartTime,
            EndTime = Input.EndTime,
            Scope = Input.Scope,
            OrganizerName = Input.OrganizerName,
            IsActive = Input.IsActive
        });

        return RedirectToPage();
    }

    private async Task<string> UploadAsync(IFormFile file, string assetType)
    {
        await using var stream = file.OpenReadStream();
        var content = new RemoteStreamContent(stream, file.FileName, file.ContentType, file.Length);
        return await _appConfigService.UploadAssetAsync(content, assetType);
    }

    public class Hl25SettingsViewModel
    {
        public string? ProgramName { get; set; }
        public string? LogoUrl { get; set; }
        public string? BannerUrl { get; set; }
        public string? TvcUrl { get; set; }
        public string? TvcHtml { get; set; }
        public string? RulesHtml { get; set; }
        public string? GamePlayHtml { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string? Scope { get; set; }
        public string? OrganizerName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
