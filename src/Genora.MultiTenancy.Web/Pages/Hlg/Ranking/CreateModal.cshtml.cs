using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Domain.Repositories;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Ranking;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgRankingInput Input { get; set; } = new();
    public List<SelectListItem> GameOptions { get; set; } = new();
    private readonly IHlgRankingAdminAppService _service;
    private readonly IRepository<HlgGame, Guid> _gameRepo;
    public CreateModalModel(IHlgRankingAdminAppService service, IRepository<HlgGame, Guid> gameRepo)
    {
        _service = service;
        _gameRepo = gameRepo;
    }
    public async Task OnGetAsync() => await LoadGamesAsync();
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await LoadGamesAsync(); return Page(); }
        await _service.CreateAsync(Input);
        return NoContent();
    }
    private async Task LoadGamesAsync()
    {
        var games = await _gameRepo.GetListAsync(x => x.IsActive);
        GameOptions = games.OrderBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), Input.GameIds.Contains(x.Id)))
            .ToList();
    }
}
