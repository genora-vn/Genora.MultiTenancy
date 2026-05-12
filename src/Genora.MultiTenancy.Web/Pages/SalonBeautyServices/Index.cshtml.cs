using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Microsoft.AspNetCore.Mvc.Rendering;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyServices;

public class IndexModel : MultiTenancyPageModel
{
    private readonly ISalonBeautyServiceCategoryAppService _categoryAppService;

    public List<SelectListItem> CategoryItems { get; set; } = new();

    public IndexModel(ISalonBeautyServiceCategoryAppService categoryAppService)
    {
        _categoryAppService = categoryAppService;
    }

    public async Task OnGetAsync()
    {
        await LoadCategoriesAsync();
    }

    private async Task LoadCategoriesAsync()
    {
        var result = await _categoryAppService.GetListAsync(new GetSalonBeautyListInput
        {
            MaxResultCount = 1000,
            Sorting = "SortOrder asc, Name asc"
        });

        CategoryItems = result.Items
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();
    }
}
