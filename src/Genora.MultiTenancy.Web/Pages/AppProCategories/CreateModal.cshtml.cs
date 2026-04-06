using Genora.MultiTenancy.AppDtos.AppProCategories;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProCategories;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateProCategoryDto Category { get; set; } = new();

    private readonly IAppProCategoryService _service;

    public CreateModalModel(IAppProCategoryService service)
        => _service = service;

    public void OnGet() => Category = new CreateUpdateProCategoryDto { IsActive = true };

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.CreateAsync(Category);
        return NoContent();
    }
}
