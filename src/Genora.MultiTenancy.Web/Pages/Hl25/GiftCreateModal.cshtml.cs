using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class GiftCreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateHl25GiftDto Gift { get; set; } = new();

    private readonly IHl25GiftAppService _giftService;

    public GiftCreateModalModel(IHl25GiftAppService giftService)
    {
        _giftService = giftService;
    }

    public void OnGet()
    {
        Gift = new CreateUpdateHl25GiftDto
        {
            Status = Hl25GiftStatus.Available
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _giftService.CreateAsync(Gift);
        return NoContent();
    }
}
