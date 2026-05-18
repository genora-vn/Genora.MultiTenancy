using Genora.MultiTenancy.AppDtos.AppPromotionPolicies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppPromotionPolicies;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppPromotionPolicyDto PromotionPolicy { get; set; } = new();

    private readonly IAppPromotionPolicyService _service;

    public CreateModalModel(IAppPromotionPolicyService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        PromotionPolicy = await _service.GetEditDataAsync(null);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.CreateAsync(PromotionPolicy);
        return NoContent();
    }
}
