using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Volo.Abp.Application.Dtos;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyDeposits;

public class IndexModel : MultiTenancyPageModel
{
    public List<SelectListItem> CustomerItems { get; set; } = new();

    private readonly ISalonBeautyCustomerAppService _customerAppService;

    public IndexModel(ISalonBeautyCustomerAppService customerAppService)
    {
        _customerAppService = customerAppService;
    }

    public async Task OnGetAsync()
    {
        var customers = await _customerAppService.GetListAsync(new AppDtos.SalonBeauties.GetSalonBeautyListInput
        {
            MaxResultCount = 200
        });
        CustomerItems = customers.Items
            .Select(x => new SelectListItem($"{x.Name} - {x.Phone}", x.Id.ToString()))
            .ToList();
    }
}
