using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Rewards;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Rewards";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgRewardDto Input { get; set; } = new();
    private readonly IHlgRewardAdminAppService _service;
    public CreateModalModel(IHlgRewardAdminAppService service) { _service = service; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.CreateAsync(Input);
        return NoContent();
    }
}
