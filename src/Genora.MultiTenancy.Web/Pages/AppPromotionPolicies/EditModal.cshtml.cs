using Genora.MultiTenancy.AppDtos.AppPromotionPolicies;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppPromotionPolicies;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppPromotionPolicyDto PromotionPolicy { get; set; } = new();

    private readonly IAppPromotionPolicyService _service;

    public EditModalModel(IAppPromotionPolicyService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        PromotionPolicy = await _service.GetEditDataAsync(Id);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Id, PromotionPolicy);
        return NoContent();
    }
}
