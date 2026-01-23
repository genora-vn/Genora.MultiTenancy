using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCustomers;

public class IndexModel : MultiTenancyPageModel
{
    public List<SelectListItem> CustomerTypeItems { get; set; } = new();

    private readonly IAppCustomerTypeService _customerTypeService;

    public IndexModel(IAppCustomerTypeService customerTypeService /* ... */)
    {
        _customerTypeService = customerTypeService;
    }

    public async Task OnGetAsync()
    {
        var result = await _customerTypeService.GetListAsync(new PagedAndSortedResultRequestDto
        {
            MaxResultCount = 1000,
            Sorting = "Name"
        });

        CustomerTypeItems = result.Items
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();
    }
}
