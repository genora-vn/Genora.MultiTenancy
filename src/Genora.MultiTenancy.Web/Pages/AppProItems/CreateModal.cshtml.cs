using Genora.MultiTenancy.AppDtos.AppProCategories;
using Genora.MultiTenancy.AppDtos.AppProItems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProItems;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateProItemDto Item { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    private readonly IAppProItemService     _itemService;
    private readonly IAppProCategoryService _categoryService;

    public CreateModalModel(IAppProItemService itemService, IAppProCategoryService categoryService)
    {
        _itemService     = itemService;
        _categoryService = categoryService;
    }

    public async Task OnGetAsync()
    {
        Item = new CreateUpdateProItemDto { IsActive = true, IsAvailable = true };
        await LoadCategoryOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _itemService.CreateAsync(Item);
        return NoContent();
    }

    private async Task LoadCategoryOptionsAsync()
    {
        var result = await _categoryService.GetListAsync(
            new GetProCategoryListInput
            { MaxResultCount = 200, SkipCount = 0, Sorting = "sortOrder asc" });

        CategoryOptions = result.Items
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();
    }
}
