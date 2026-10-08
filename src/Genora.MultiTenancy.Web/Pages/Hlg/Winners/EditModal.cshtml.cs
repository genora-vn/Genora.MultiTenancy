using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Winners;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgWinnerInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    public List<HlgEndedGameLookupDto> EndedGames { get; set; } = new();
    private readonly IHlgWinnerAdminAppService _service;
    private readonly IStringLocalizer<MultiTenancyResource> _l;
    public EditModalModel(IHlgWinnerAdminAppService service, IStringLocalizer<MultiTenancyResource> l) { _service=service; _l=l; }
    public async Task OnGetAsync()
    {
        var item = await _service.GetAsync(Id);
        Input = new UpdateHlgWinnerInput { EventId = item.EventId, GameId = item.GameId, PrizeId = item.PrizeId, CustomerId = item.CustomerId, IsActive = item.IsActive };
        if (item.EventId != Guid.Empty) EndedGames = await _service.GetEndedEventGamesAsync(item.EventId);
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.GameId == Guid.Empty) ModelState.AddModelError("Input.GameId", _l["Hlg:GameRequired"].Value);
        if (!ModelState.IsValid)
        {
            if (Input.EventId != Guid.Empty) EndedGames = await _service.GetEndedEventGamesAsync(Input.EventId);
            return Page();
        }
        await _service.UpdateAsync(Id, Input);
        return NoContent();
    }
}
