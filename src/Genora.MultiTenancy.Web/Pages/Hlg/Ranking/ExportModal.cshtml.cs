using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc.Rendering;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Ranking;
public class ExportModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    public string EventTitle { get; set; } = "";
    public bool CanFinalize { get; set; }
    public List<SelectListItem> GameOptions { get; set; } = new();
    private readonly IHlgRankingAdminAppService _service;
    public ExportModalModel(IHlgRankingAdminAppService service) { _service = service; }
    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        EventTitle = dto.Title;
        CanFinalize = dto.CanExportResults; // = Clock.Now > EndAt (sự kiện đã kết thúc)
        var games = await _service.GetEndedCampaignGamesAsync(Id);
        GameOptions = games.Select(g => new SelectListItem(g.Name, g.Id.ToString(), true)).ToList();
    }
}
