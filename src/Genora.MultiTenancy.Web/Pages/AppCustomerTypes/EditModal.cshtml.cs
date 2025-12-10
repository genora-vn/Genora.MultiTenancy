using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppCustomerTypes;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppCustomerTypeDto CustomerType { get; set; }

    private readonly IAppCustomerTypeService _appCustomerTypeService;

    public EditModalModel(IAppCustomerTypeService appCustomerTypeService)
    {
        _appCustomerTypeService = appCustomerTypeService;
    }

    public async Task OnGetAsync()
    {
        var appCustomerTypeDto = await _appCustomerTypeService.GetAsync(Id);
        CustomerType = ObjectMapper.Map<AppCustomerTypeDto, CreateUpdateAppCustomerTypeDto>(appCustomerTypeDto);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appCustomerTypeService.UpdateAsync(Id, CustomerType);
        return NoContent();
    }
}