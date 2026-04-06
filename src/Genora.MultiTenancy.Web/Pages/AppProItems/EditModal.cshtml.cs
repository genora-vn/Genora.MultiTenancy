using Genora.MultiTenancy.AppDtos.AppProCategories;
using Genora.MultiTenancy.AppDtos.AppProItems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProItems;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateProItemDto Item { get; set; } = new();

    public List<SelectListItem> CategoryOptions { get; set; } = new();

    private readonly IAppProItemService     _itemService;
    private readonly IAppProCategoryService _categoryService;

    public EditModalModel(IAppProItemService itemService, IAppProCategoryService categoryService)
    {
        _itemService     = itemService;
        _categoryService = categoryService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _itemService.GetAsync(Id);
        Item = ObjectMapper.Map<ProItemDto, CreateUpdateProItemDto>(dto);
        await LoadCategoryOptionsAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _itemService.UpdateAsync(Id, Item);
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
