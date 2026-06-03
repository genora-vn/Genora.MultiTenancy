using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.AppCaddies;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateCaddieDto Caddie { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

    [BindProperty]
    public List<Guid> SelectedLanguageIds { get; set; } = new();

    [BindProperty]
    public List<byte> SelectedVoiceRegions { get; set; } = new();

    public string CaddieCode { get; set; } = string.Empty;
    public string? CurrentAvatarUrl { get; set; }
    public List<SelectListItem> GenderItems { get; set; } = new();
    public List<SelectListItem> VoiceRegionItems { get; set; } = new();
    public List<SelectListItem> LanguageItems { get; set; } = new();

    private readonly CaddieAppService _caddieAppService;
    private readonly CaddieLanguageAppService _languageAppService;

    public EditModalModel(
        CaddieAppService caddieAppService,
        CaddieLanguageAppService languageAppService)
    {
        _caddieAppService = caddieAppService;
        _languageAppService = languageAppService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _caddieAppService.GetAsync(Id);

        CaddieCode = dto.CaddieCode;
        CurrentAvatarUrl = dto.Avatar;

        Caddie = new CreateUpdateCaddieDto
        {
            CaddieName = dto.CaddieName,
            AvatarUrl = dto.Avatar,
            Gender = dto.Gender,
            Phone = dto.Phone,
            GolfCourseId = dto.GolfCourseId,
            JoinDate = dto.JoinDate,
            HeightCm = dto.HeightCm,
            Status = dto.Status,
            IsShowOnApp = dto.IsShowOnApp,
            Note = dto.Note
        };

        SelectedLanguageIds = dto.LanguageIds;
        SelectedVoiceRegions = dto.VoiceRegionValues;

        await BuildSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Caddie.LanguageIds = SelectedLanguageIds ?? new();
        Caddie.VoiceRegions = SelectedVoiceRegions ?? new();

        // Convert IFormFile to IRemoteStreamContent for the service
        if (AvatarFile != null && AvatarFile.Length > 0)
        {
            Caddie.AvatarFile = new RemoteStreamContent(
                AvatarFile.OpenReadStream(),
                AvatarFile.FileName,
                AvatarFile.ContentType,
                AvatarFile.Length);
        }

        await _caddieAppService.UpdateAsync(Id, Caddie);
        return NoContent();
    }

    private async Task BuildSelectListsAsync()
    {
        GenderItems = new List<SelectListItem>
        {
            new("Nam", ((byte)CaddieGender.Male).ToString()),
            new("Nữ", ((byte)CaddieGender.Female).ToString())
        };

        VoiceRegionItems = new List<SelectListItem>
        {
            new("Miền Bắc", ((byte)CaddieVoiceRegion.North).ToString()),
            new("Miền Trung", ((byte)CaddieVoiceRegion.Central).ToString()),
            new("Miền Nam", ((byte)CaddieVoiceRegion.South).ToString())
        };

        var languages = await _languageAppService.GetAllActiveAsync();
        LanguageItems = languages.Items
            .Select(x => new SelectListItem(x.LanguageName, x.Id.ToString()))
            .ToList();
    }
}
