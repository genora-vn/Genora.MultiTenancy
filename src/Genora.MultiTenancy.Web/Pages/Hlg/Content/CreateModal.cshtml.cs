using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Content;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Content";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgContentInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgContentAdminAppService _service;
    public CreateModalModel(IHlgContentAdminAppService service) { _service=service; }
    public void OnGet(Guid? parentId) {  }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.CreateAsync(Input); return NoContent(); }
}
