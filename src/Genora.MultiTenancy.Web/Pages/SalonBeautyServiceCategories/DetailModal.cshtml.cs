using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyServiceCategories;

public class DetailModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public SalonBeautyServiceCategoryDto Category { get; set; } = new();

    private readonly ISalonBeautyServiceCategoryAppService _categoryAppService;

    public DetailModalModel(ISalonBeautyServiceCategoryAppService categoryAppService)
    {
        _categoryAppService = categoryAppService;
    }

    public async Task OnGetAsync()
    {
        Category = await _categoryAppService.GetAsync(Id);
    }
}
