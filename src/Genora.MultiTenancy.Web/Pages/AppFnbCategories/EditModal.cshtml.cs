using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbCategories;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateFnbCategoryDto Category { get; set; } = new();

    private readonly IAppFnbCategoryService _service;

    public EditModalModel(IAppFnbCategoryService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Category = ObjectMapper.Map<FnbCategoryDto, CreateUpdateFnbCategoryDto>(dto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateAsync(Id, Category);
        return NoContent();
    }
}