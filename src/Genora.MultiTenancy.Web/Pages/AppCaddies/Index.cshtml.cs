using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppServices.Caddies;
using Genora.MultiTenancy.AppDtos.Caddies;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.AppCaddies;

public class IndexModel : MultiTenancyPageModel
{
    public List<SelectListItem> GolfCourseItems { get; set; } = new();
    public List<SelectListItem> LanguageItems { get; set; } = new();

    private readonly CaddieLanguageAppService _languageAppService;

    public IndexModel(CaddieLanguageAppService languageAppService)
    {
        _languageAppService = languageAppService;
    }

    public async Task OnGetAsync()
    {
        var languages = await _languageAppService.GetAllActiveAsync();
        LanguageItems = languages.Items
            .Select(x => new SelectListItem(x.LanguageName, x.Id.ToString()))
            .ToList();
    }
}
