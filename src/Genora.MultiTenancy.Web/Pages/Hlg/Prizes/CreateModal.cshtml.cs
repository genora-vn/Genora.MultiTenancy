using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Prizes;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Ranking";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgPrizeInput Input { get; set; } = new();
    [BindProperty(SupportsGet=true)] public Guid Id { get; set; }
    private readonly IHlgPrizeAdminAppService _service;
    public CreateModalModel(IHlgPrizeAdminAppService service) { _service=service; }
    public void OnGet(Guid? parentId) { Input.EventId=parentId ?? Guid.Empty; }
    public async Task<IActionResult> OnPostAsync() { if(!ModelState.IsValid) return Page(); await _service.CreateAsync(Input); return NoContent(); }
}
