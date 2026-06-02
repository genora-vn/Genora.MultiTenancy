using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.AppCaddies;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateCaddieDto Caddie { get; set; } = new();

    [BindProperty]
    public List<Guid> SelectedLanguageIds { get; set; } = new();

    [BindProperty]
    public List<byte> SelectedVoiceRegions { get; set; } = new();

    public List<SelectListItem> GenderItems { get; set; } = new();
    public List<SelectListItem> VoiceRegionItems { get; set; } = new();
    public List<SelectListItem> LanguageItems { get; set; } = new();
    public string GeneratedCode { get; set; } = string.Empty;

    private readonly CaddieAppService _caddieAppService;
    private readonly CaddieLanguageAppService _languageAppService;

    public CreateModalModel(
        CaddieAppService caddieAppService,
        CaddieLanguageAppService languageAppService)
    {
        _caddieAppService = caddieAppService;
        _languageAppService = languageAppService;
    }

    public async Task OnGetAsync()
    {
        Caddie = new CreateUpdateCaddieDto
        {
            Status = 1,
            IsShowOnApp = true
        };

        GeneratedCode = await _caddieAppService.GenerateCaddieCodeAsync();
        await BuildSelectListsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Caddie.LanguageIds = SelectedLanguageIds ?? new();
        Caddie.VoiceRegions = SelectedVoiceRegions ?? new();

        await _caddieAppService.CreateAsync(Caddie);
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
