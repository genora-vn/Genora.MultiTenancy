using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Ranking;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgRankingInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    private readonly IHlgRankingAdminAppService _service;
    public EditModalModel(IHlgRankingAdminAppService service) { _service = service; }
    public async Task OnGetAsync()
    {
        var item = await _service.GetAsync(Id);
        Input = new UpdateHlgRankingInput
        {
            Title = item.Title,
            Description = item.Description,
            StartAt = item.StartAt,
            EndAt = item.EndAt,
            IsActive = item.IsActive,
        };
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.UpdateAsync(Id, Input);
        return NoContent();
    }
}
