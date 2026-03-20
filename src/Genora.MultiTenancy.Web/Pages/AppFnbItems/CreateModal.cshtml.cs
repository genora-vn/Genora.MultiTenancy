using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbItems;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateFnbItemDto Item { get; set; } = new();

    public SelectList CategorySelectList { get; set; } = default!;

    private readonly IAppFnbItemService _service;
    private readonly IAppFnbCategoryService _categoryService;

    public CreateModalModel(
        IAppFnbItemService service,
        IAppFnbCategoryService categoryService)
    {
        _service = service;
        _categoryService = categoryService;
    }

    public async Task OnGetAsync()
    {
        Item = new CreateUpdateFnbItemDto
        {
            IsActive = true,
            IsAvailable = true
        };

        await LoadCategoriesAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.CreateAsync(Item);
        return NoContent();
    }

    private async Task LoadCategoriesAsync()
    {
        var result = await _categoryService.GetListAsync(new GetFnbCategoryListInput
        {
            SkipCount = 0,
            MaxResultCount = 1000,
            Sorting = "SortOrder asc",
            IsActive = true
        });

        CategorySelectList = new SelectList(result.Items.ToList(), "Id", "Name");
    }
}