using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyServiceCategories;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateSalonBeautyServiceCategoryDto Category { get; set; } = new();

    private readonly ISalonBeautyServiceCategoryAppService _categoryAppService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public CreateModalModel(
        ISalonBeautyServiceCategoryAppService categoryAppService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _categoryAppService = categoryAppService;
        _l = l;
    }

    public void OnGet()
    {
        Category = new CreateSalonBeautyServiceCategoryDto
        {
            Status = 1,
            SortOrder = 1
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateCategoryInput(Category.Name, Category.SortOrder, Category.Status);

        if (!ModelState.IsValid)
            return Page();

        await _categoryAppService.CreateAsync(Category);
        return NoContent();
    }

    private void ValidateCategoryInput(string? name, int sortOrder, byte status)
    {
        if (string.IsNullOrWhiteSpace(name))
            ModelState.AddModelError("Category.Name", _l["SalonBeautyServiceCategories:NameRequired"]);

        if (sortOrder < 0)
            ModelState.AddModelError("Category.SortOrder", _l["SalonBeautyServiceCategories:SortOrderInvalid"]);

        if (status != 0 && status != 1)
            ModelState.AddModelError("Category.Status", _l["SalonBeautyServiceCategories:StatusInvalid"]);
    }
}
