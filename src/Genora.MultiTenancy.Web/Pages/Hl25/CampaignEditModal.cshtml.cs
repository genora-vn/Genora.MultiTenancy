using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class CampaignEditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateHl25FrameCampaignDto Campaign { get; set; } = new();

    private readonly IHl25FrameCampaignAppService _campaignService;

    public CampaignEditModalModel(IHl25FrameCampaignAppService campaignService)
    {
        _campaignService = campaignService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _campaignService.GetAsync(Id);
        Campaign = new CreateUpdateHl25FrameCampaignDto
        {
            Name = dto.Name,
            Description = dto.Description,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            Status = dto.Status
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _campaignService.UpdateAsync(Id, Campaign);
        return NoContent();
    }
}
