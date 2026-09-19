using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Prizes;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgPrizeInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgPrizeAdminAppService _service;
    public EditModalModel(IHlgPrizeAdminAppService service) { _service=service; }
    public async Task OnGetAsync() { var item=await _service.GetAsync(Id); Input=new UpdateHlgPrizeInput {             EventId = item.EventId,
            RewardId = item.RewardId,
            Title = item.Title,
            Quantity = item.Quantity,
            DisplayOrder = item.DisplayOrder,
            IsActive = item.IsActive, }; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.UpdateAsync(Id, Input); return NoContent(); }
}
