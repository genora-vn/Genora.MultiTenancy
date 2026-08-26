using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class TemplateCreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateHl25FrameTemplateDto Template { get; set; } = new();

    [BindProperty]
    public IFormFile? ImageFile { get; set; }

    public List<SelectListItem> CampaignItems { get; set; } = new();

    private readonly IHl25FrameTemplateAppService _templateService;
    private readonly IHl25FrameCampaignAppService _campaignService;

    public TemplateCreateModalModel(
        IHl25FrameTemplateAppService templateService,
        IHl25FrameCampaignAppService campaignService)
    {
        _templateService = templateService;
        _campaignService = campaignService;
    }

    public async Task OnGetAsync()
    {
        await LoadCampaignsAsync();
        Template = new CreateUpdateHl25FrameTemplateDto
        {
            IsActive = true,
            DisplayOrder = 0
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (ImageFile != null && ImageFile.Length > 0)
        {
            await using var stream = ImageFile.OpenReadStream();
            var content = new RemoteStreamContent(stream, ImageFile.FileName, ImageFile.ContentType, ImageFile.Length);
            Template.ImageUrl = await _templateService.UploadTemplateImageAsync(content);
        }

        await _templateService.CreateAsync(Template);
        return NoContent();
    }

    private async Task LoadCampaignsAsync()
    {
        var campaigns = await _campaignService.GetListAsync(new GetHl25FrameCampaignListInput { MaxResultCount = 1000 });
        CampaignItems = campaigns.Items
            .Select(c => new SelectListItem(c.Name, c.Id.ToString()))
            .ToList();
    }
}
