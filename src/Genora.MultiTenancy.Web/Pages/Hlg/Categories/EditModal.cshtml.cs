using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Categories;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgCategoryInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    private readonly IHlgCategoryAdminAppService _service;
    public EditModalModel(IHlgCategoryAdminAppService service) { _service = service; }
    public async Task OnGetAsync()
    {
        var item = await _service.GetAsync(Id);
        Input = new UpdateHlgCategoryInput
        {
            Name = item.Name,
            Description = item.Description,
            ImageUrl = item.ImageUrl,
            DisplayOrder = item.DisplayOrder,
            IsActive = item.IsActive,
        };
    }
    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();
        await _service.UpdateAsync(Id, Input);
        return NoContent();
    }
}
