using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class CampaignCreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateHl25FrameCampaignDto Campaign { get; set; } = new();

    private readonly IHl25FrameCampaignAppService _campaignService;

    public CampaignCreateModalModel(IHl25FrameCampaignAppService campaignService)
    {
        _campaignService = campaignService;
    }

    public void OnGet()
    {
        Campaign = new CreateUpdateHl25FrameCampaignDto
        {
            Status = Hl25CampaignStatus.Draft
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _campaignService.CreateAsync(Campaign);
        return NoContent();
    }
}
