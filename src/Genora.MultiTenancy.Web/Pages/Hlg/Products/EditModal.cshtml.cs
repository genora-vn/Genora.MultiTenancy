using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Microsoft.AspNetCore.Mvc;
namespace Genora.MultiTenancy.Web.Pages.Hlg.Products;
public class EditModalModel : HlgAdminPageModel
{
    protected override string PermissionGroup => "Knowledge";
    protected override string ActionSuffix => ".Edit";
    [BindProperty] public UpdateHlgProductInput Input { get; set; } = new();
    [BindProperty(SupportsGet = true)] public Guid Id { get; set; }
    private readonly IHlgProductAdminAppService _service;
    public EditModalModel(IHlgProductAdminAppService service) { _service = service; }
    public async Task OnGetAsync()
    {
        var item = await _service.GetAsync(Id);
        Input = new UpdateHlgProductInput
        {
            BrandId = item.BrandId, Details = item.Details,
            CategoryId = item.CategoryId,
            Name = item.Name,
            ThumbnailUrl = item.ThumbnailUrl,
            Summary = item.Summary,
            Content = item.Content,
            ImageUrls = item.ImageUrls,
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
