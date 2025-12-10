using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppCustomerTypes;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppCustomerTypeDto CustomerType { get; set; }

    private readonly IAppCustomerTypeService _appCustomerTypeService;

    public CreateModalModel(IAppCustomerTypeService appCustomerTypeService)
    {
        _appCustomerTypeService = appCustomerTypeService;
    }

    public void OnGet()
    {
        CustomerType = new CreateUpdateAppCustomerTypeDto();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appCustomerTypeService.CreateAsync(CustomerType);
        return NoContent();
    }
}