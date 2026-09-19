using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Games;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Games";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgGameInput Input { get; set; } = new();
    private readonly IHlgGameAdminAppService _service;
    public CreateModalModel(IHlgGameAdminAppService service) { _service = service; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.CreateAsync(Input);
        return NoContent();
    }
}
