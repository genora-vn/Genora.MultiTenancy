using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCustomers;

public class IndexModel : MultiTenancyPageModel
{
    public List<SelectListItem> CustomerTypeItems { get; set; } = new();
    public List<SelectListItem> CustomerSourceItems { get; set; } = new();

    private readonly IMiniAppCustomerTypeService _customerTypeService;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public IndexModel(
        IMiniAppCustomerTypeService customerTypeService,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _customerTypeService = customerTypeService;
        _l = l;
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

        CustomerSourceItems = Enum.GetValues<CustomerSource>()
            .Select(s => new SelectListItem(
                _l[$"CustomerSource:{s}"],
                ((int)s).ToString()))
            .ToList();
    }
}
