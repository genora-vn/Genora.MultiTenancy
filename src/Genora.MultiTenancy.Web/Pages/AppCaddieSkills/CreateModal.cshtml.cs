using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.AppServices.Caddies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace Genora.MultiTenancy.Web.Pages.AppCaddieSkills;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateCaddieSkillDto Skill { get; set; } = new();

    public Task OnGetAsync()
    {
        Skill = new CreateUpdateCaddieSkillDto { Status = 1, SortOrder = 0 };
        return Task.CompletedTask;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        var service = HttpContext.RequestServices.GetRequiredService<CaddieSkillAppService>();
        await service.CreateAsync(Skill);
        return NoContent();
    }
}
