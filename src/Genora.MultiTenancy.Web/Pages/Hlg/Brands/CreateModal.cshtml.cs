using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Brands;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgBrandInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgBrandAdminAppService _service;
    public CreateModalModel(IHlgBrandAdminAppService service) { _service=service; }
    public void OnGet(Guid? parentId) { Input.CategoryId=parentId ?? Guid.Empty; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.CreateAsync(Input); return NoContent(); }
}
