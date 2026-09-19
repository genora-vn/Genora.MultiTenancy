using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Products;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgProductInput Input { get; set; } = new();
    private readonly IHlgProductAdminAppService _service;
    public CreateModalModel(IHlgProductAdminAppService service) { _service = service; }
    public void OnGet(Guid parentId) { Input.CategoryId = parentId; }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.CreateAsync(Input);
        return NoContent();
    }
}
