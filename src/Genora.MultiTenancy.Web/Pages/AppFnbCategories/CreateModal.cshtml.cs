using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbCategories;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateFnbCategoryDto Category { get; set; } = new();

    private readonly IAppFnbCategoryService _service;

    public CreateModalModel(IAppFnbCategoryService service)
    {
        _service = service;
    }

    public void OnGet()
    {
        Category = new CreateUpdateFnbCategoryDto
        {
            IsActive = true
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.CreateAsync(Category);
        return NoContent();
    }
}