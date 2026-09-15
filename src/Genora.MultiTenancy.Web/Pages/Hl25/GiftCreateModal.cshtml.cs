using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class GiftCreateModalModel : Hl25GiftModalModelBase
{
    public GiftCreateModalModel(IHl25GiftAppService giftService) : base(giftService) { }

    public void OnGet()
    {
        Gift = new CreateUpdateHl25GiftDto
        {
            Status = Hl25GiftStatus.Available
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await PrepareGiftAsync();
        await GiftService.CreateAsync(Gift);
        return NoContent();
    }
}
