using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppMembershipTiers;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppMembershipTierDto MembershipTier { get; set; }

    private readonly IAppMembershipTierService _appMembershipTierService;

    public EditModalModel(IAppMembershipTierService appMembershipTierService)
    {
        _appMembershipTierService = appMembershipTierService;
    }

    public async Task OnGetAsync()
    {
        var appMembershipTierDto = await _appMembershipTierService.GetAsync(Id);
        MembershipTier = ObjectMapper.Map<AppMembershipTierDto, CreateUpdateAppMembershipTierDto>(appMembershipTierDto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appMembershipTierService.UpdateAsync(Id, MembershipTier);
        return NoContent();
    }
}