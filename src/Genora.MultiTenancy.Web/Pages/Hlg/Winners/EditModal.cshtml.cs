using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Winners;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgWinnerInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgWinnerAdminAppService _service;
    public EditModalModel(IHlgWinnerAdminAppService service) { _service=service; }
    public async Task OnGetAsync() { var item=await _service.GetAsync(Id); Input=new UpdateHlgWinnerInput {             EventId = item.EventId,
            PrizeId = item.PrizeId,
            CustomerId = item.CustomerId,
            IsActive = item.IsActive, }; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.UpdateAsync(Id, Input); return NoContent(); }
}
