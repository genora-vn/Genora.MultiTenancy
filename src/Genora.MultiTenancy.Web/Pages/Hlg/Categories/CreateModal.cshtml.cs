using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Categories;
public class CreateModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    protected override string ActionSuffix => ".Create";
    [BindProperty] public CreateHlgCategoryInput Input { get; set; } = new();
    private readonly IHlgCategoryAdminAppService _service;
    public CreateModalModel(IHlgCategoryAdminAppService service) { _service = service; }
    public void OnGet() { }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.CreateAsync(Input);
        return NoContent();
    }
}
