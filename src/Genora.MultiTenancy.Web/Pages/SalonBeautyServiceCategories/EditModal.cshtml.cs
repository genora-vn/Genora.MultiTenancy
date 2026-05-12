using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyServiceCategories;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateSalonBeautyServiceCategoryDto Category { get; set; } = new();

    private readonly ISalonBeautyServiceCategoryAppService _categoryAppService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public EditModalModel(
        ISalonBeautyServiceCategoryAppService categoryAppService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _categoryAppService = categoryAppService;
        _l = l;
    }

    public async Task OnGetAsync()
    {
        var dto = await _categoryAppService.GetAsync(Id);
        Category = new UpdateSalonBeautyServiceCategoryDto
        {
            Name = dto.Name,
            Description = dto.Description,
            SortOrder = dto.SortOrder,
            Status = dto.Status,
            Note = dto.Note
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        ValidateCategoryInput(Category.Name, Category.SortOrder, Category.Status);

        if (!ModelState.IsValid)
            return Page();

        await _categoryAppService.UpdateAsync(Id, Category);
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
