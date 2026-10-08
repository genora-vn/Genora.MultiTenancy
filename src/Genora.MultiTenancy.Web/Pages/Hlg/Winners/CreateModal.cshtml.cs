using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Winners;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgWinnersBatchInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    public List<HlgEndedGameLookupDto> EndedGames { get; set; } = new();
    private readonly IHlgWinnerAdminAppService _service;
    private readonly IStringLocalizer<MultiTenancyResource> _l;
    public CreateModalModel(IHlgWinnerAdminAppService service, IStringLocalizer<MultiTenancyResource> l) { _service=service; _l=l; }
    public async Task OnGetAsync(Guid? parentId)
    {
        Input.EventId = parentId ?? Guid.Empty;
        if (Input.EventId != Guid.Empty) EndedGames = await _service.GetEndedEventGamesAsync(Input.EventId);
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (Input.GameId == Guid.Empty) ModelState.AddModelError("Input.GameId", _l["Hlg:GameRequired"].Value);
        if (Input.CustomerIds == null || Input.CustomerIds.Count == 0) ModelState.AddModelError("Input.CustomerIds", _l["Hlg:PlayersRequired"].Value);
        if (!ModelState.IsValid)
        {
            if (Input.EventId != Guid.Empty) EndedGames = await _service.GetEndedEventGamesAsync(Input.EventId);
            return Page();
        }
        await _service.CreateManyAsync(Input);
        return NoContent();
    }
}
