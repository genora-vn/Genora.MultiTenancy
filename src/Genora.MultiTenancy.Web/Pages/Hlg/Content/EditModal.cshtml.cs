using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Content;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Content";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgContentInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgContentAdminAppService _service;
    public EditModalModel(IHlgContentAdminAppService service) { _service=service; }
    public async Task OnGetAsync() { var item=await _service.GetAsync(Id); Input=new UpdateHlgContentInput {             Slot = item.Slot,
            Title = item.Title,
            Summary = item.Summary,
            BadgeText = item.BadgeText,
            ImageUrl = item.ImageUrl,
            TargetUrl = item.TargetUrl,
            GameId = item.GameId,
            DisplayOrder = item.DisplayOrder,
            IsActive = item.IsActive, }; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.UpdateAsync(Id, Input); return NoContent(); }
}
