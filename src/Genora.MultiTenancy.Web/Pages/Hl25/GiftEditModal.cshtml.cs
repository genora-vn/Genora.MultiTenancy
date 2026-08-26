using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hl25;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.Hl25;

public class GiftEditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateHl25GiftDto Gift { get; set; } = new();

    private readonly IHl25GiftAppService _giftService;

    public GiftEditModalModel(IHl25GiftAppService giftService)
    {
        _giftService = giftService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _giftService.GetAsync(Id);
        Gift = new CreateUpdateHl25GiftDto
        {
            Name = dto.Name,
            ImageUrl = dto.ImageUrl,
            Description = dto.Description,
            TotalQuantity = dto.TotalQuantity,
            RemainingQuantity = dto.RemainingQuantity,
            Value = dto.Value,
            Status = dto.Status
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _giftService.UpdateAsync(Id, Gift);
        return NoContent();
    }
}
