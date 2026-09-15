using System;
using System.Globalization;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class GiftEditModalModel : Hl25GiftModalModelBase
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public GiftEditModalModel(IHl25GiftAppService giftService) : base(giftService) { }

    public async Task OnGetAsync()
    {
        var dto = await GiftService.GetAsync(Id);
        Gift = new CreateUpdateHl25GiftDto
        {
            Name = dto.Name,
            ImageUrl = dto.ImageUrl,
            WheelImageUrl = dto.WheelImageUrl,
            Description = dto.Description,
            TotalQuantity = dto.TotalQuantity,
            RemainingQuantity = dto.RemainingQuantity,
            Value = dto.Value,
            Status = dto.Status
        };
        GiftValue = dto.Value?.ToString("#,0.##", CultureInfo.GetCultureInfo("vi-VN"));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await PrepareGiftAsync();
        await GiftService.UpdateAsync(Id, Gift);
        return NoContent();
    }
}
