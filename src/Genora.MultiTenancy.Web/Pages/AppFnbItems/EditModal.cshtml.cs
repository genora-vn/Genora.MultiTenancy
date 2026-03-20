using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbItems;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateFnbItemDto Item { get; set; } = new();

    public SelectList CategorySelectList { get; set; } = default!;

    private readonly IAppFnbItemService _service;
    private readonly IAppFnbCategoryService _categoryService;

    public EditModalModel(
        IAppFnbItemService service,
        IAppFnbCategoryService categoryService)
    {
        _service = service;
        _categoryService = categoryService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Item = ObjectMapper.Map<FnbItemDto, CreateUpdateFnbItemDto>(dto);
        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Id, Item);
        return NoContent();
    }

    private async Task LoadCategoriesAsync()
    {
        var result = await _categoryService.GetListAsync(new GetFnbCategoryListInput
        {
            SkipCount = 0,
            MaxResultCount = 1000,
            Sorting = "SortOrder asc"
        });

        CategorySelectList = new SelectList(result.Items.ToList(), "Id", "Name");
    }
}