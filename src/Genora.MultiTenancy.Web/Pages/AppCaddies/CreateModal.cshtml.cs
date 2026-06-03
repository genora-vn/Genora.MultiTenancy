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

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateCaddieDto Caddie { get; set; } = new();

    [BindProperty]
    public IFormFile? AvatarFile { get; set; }

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
        try
        {
            // Map selected items to DTO
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

            // Validate
            if (string.IsNullOrWhiteSpace(Caddie.CaddieName))
            {
                ModelState.AddModelError("Caddie.CaddieName", "Tên Caddy không được để trống");
                return Page();
            }

            // Create
            await _caddieAppService.CreateAsync(Caddie);
            return NoContent();
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return Page();
        }
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
