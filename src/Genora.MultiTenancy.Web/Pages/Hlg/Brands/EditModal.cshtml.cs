using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Brands;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgBrandInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgBrandAdminAppService _service;
    public EditModalModel(IHlgBrandAdminAppService service) { _service=service; }
    public async Task OnGetAsync() { var item=await _service.GetAsync(Id); Input=new UpdateHlgBrandInput {             CategoryId = item.CategoryId,
            Name = item.Name,
            DisplayOrder = item.DisplayOrder,
            IsActive = item.IsActive, }; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.UpdateAsync(Id, Input); return NoContent(); }
}
