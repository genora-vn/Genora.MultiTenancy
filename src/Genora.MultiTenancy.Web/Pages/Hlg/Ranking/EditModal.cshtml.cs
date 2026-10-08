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
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgRankingInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public List<SelectListItem> GameOptions { get; set; } = new();
    private readonly IHlgRankingAdminAppService _service;
    private readonly IRepository<HlgGame, Guid> _gameRepo;
    public EditModalModel(IHlgRankingAdminAppService service, IRepository<HlgGame, Guid> gameRepo)
    {
        _service = service;
        _gameRepo = gameRepo;
    }
    public async Task OnGetAsync()
    {
        var item = await _service.GetAsync(Id);
        Input = new UpdateHlgRankingInput
        {
            Title = item.Title,
            Description = item.Description,
            StartAt = item.StartAt,
            EndAt = item.EndAt,
            GameId = item.GameId,
            GameIds = item.GameIds,
            IsActive = item.IsActive,
        };
        await LoadGamesAsync();
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) { await LoadGamesAsync(); return Page(); }
        await _service.UpdateAsync(Id, Input);
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
