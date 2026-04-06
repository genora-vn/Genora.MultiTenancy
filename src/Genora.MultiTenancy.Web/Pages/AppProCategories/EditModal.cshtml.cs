using Genora.MultiTenancy.AppDtos.AppProCategories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProCategories;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateProCategoryDto Category { get; set; } = new();

    private readonly IAppProCategoryService _service;

    public EditModalModel(IAppProCategoryService service)
        => _service = service;

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Category = ObjectMapper.Map<ProCategoryDto, CreateUpdateProCategoryDto>(dto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Id, Category);
        return NoContent();
    }
}
