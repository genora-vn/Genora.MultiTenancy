using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Genora.MultiTenancy.Web.Pages.AppLanguages;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateLanguageDto Language { get; set; } = new();

    public Task OnGetAsync()
    {
        Language = new CreateUpdateLanguageDto { Status = 1, SortOrder = 0 };
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var service = HttpContext.RequestServices.GetRequiredService<CaddieLanguageAppService>();
        await service.CreateAsync(Language);
        return NoContent();
    }
}
