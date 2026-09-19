using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Winners;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgWinnerInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgWinnerAdminAppService _service;
    public CreateModalModel(IHlgWinnerAdminAppService service) { _service=service; }
    public void OnGet(Guid? parentId) { Input.EventId=parentId ?? Guid.Empty; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.CreateAsync(Input); return NoContent(); }
}
