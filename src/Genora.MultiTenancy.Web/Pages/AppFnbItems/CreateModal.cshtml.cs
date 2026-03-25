using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbItems;
public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateFnbItemDto Item { get; set; } = new();

    public SelectList CategorySelectList { get; set; } = default!;

    private readonly IAppFnbItemService _itemService;
    private readonly IAppFnbCategoryService _categoryService;

    private const long MaxImageBytes = 15L * 1024 * 1024;

    public CreateModalModel(
        IAppFnbItemService itemService,
        IAppFnbCategoryService categoryService)
    {
        _itemService = itemService;
        _categoryService = categoryService;
    }

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();

        Item = new CreateUpdateFnbItemDto
        {
            IsActive = true,
            IsAvailable = true,
            IsUploadImage = true
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await LoadCategoriesAsync();

        if (Item == null) Item = new CreateUpdateFnbItemDto();

        if (Item.IsUploadImage)
        {
            var len = Item.Images?.ContentLength ?? 0;

            if (len <= 0)
            {
                ModelState.AddModelError("Item.Images", "Vui lòng chọn ảnh để upload trước khi lưu.");
            }
            else if (len > MaxImageBytes)
            {
                ModelState.AddModelError("Item.Images", "Ảnh vượt quá 15MB. Vui lòng chọn ảnh nhỏ hơn.");
            }

            var ct = Item.Images?.ContentType ?? "";
            if (len > 0 && !ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("Item.Images", "File không phải ảnh hợp lệ.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(Item.ImageUrl))
            {
                Item.ImageUrl = null;
            }

            Item.Images = null;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _itemService.CreateAsync(Item);
        return NoContent();
    }

    private async Task LoadCategoriesAsync()
    {
        var result = await _categoryService.GetListAsync(new GetFnbCategoryListInput
        {
            MaxResultCount = 1000,
            SkipCount = 0,
            IsActive = true,
            Sorting = "SortOrder asc"
        });

        CategorySelectList = new SelectList(
            result.Items.Select(x => new { x.Id, x.Name }),
            "Id",
            "Name"
        );
    }
}