using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppMembershipTiers;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppMembershipTierDto MembershipTier { get; set; }

    private readonly IAppMembershipTierService _appMembershipTierService;

    public CreateModalModel(IAppMembershipTierService appMembershipTierService)
    {
        _appMembershipTierService = appMembershipTierService;
    }

    public void OnGet()
    {
        MembershipTier = new CreateUpdateAppMembershipTierDto
        {
            IsActive = true,
            DisplayOrder = 0
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appMembershipTierService.CreateAsync(MembershipTier);
        return NoContent();
    }
}