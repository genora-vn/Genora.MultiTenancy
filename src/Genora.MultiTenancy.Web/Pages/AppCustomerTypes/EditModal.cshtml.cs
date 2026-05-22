using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppSpecialDates;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppCustomerTypes;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppCustomerTypeDto CustomerType { get; set; }

    public List<CustomerTypeOriginalPriceFieldDto> PriceFields { get; set; } = new();

    private readonly IAppCustomerTypeService _appCustomerTypeService;
    private readonly IAppSpecialDateService _appSpecialDateService;

    public EditModalModel(
        IAppCustomerTypeService appCustomerTypeService,
        IAppSpecialDateService appSpecialDateService)
    {
        _appCustomerTypeService = appCustomerTypeService;
        _appSpecialDateService = appSpecialDateService;
    }

    public async Task OnGetAsync()
    {
        var appCustomerTypeDto = await _appCustomerTypeService.GetAsync(Id);
        CustomerType = ObjectMapper.Map<AppCustomerTypeDto, CreateUpdateAppCustomerTypeDto>(appCustomerTypeDto);

        var specialDates = await _appSpecialDateService.GetListAsync(new GetSpecialDateListInput
        {
            MaxResultCount = 100
        });

        PriceFields = CreateModalModel.BuildFields(specialDates, CustomerType);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _appCustomerTypeService.UpdateAsync(Id, CustomerType);
        return NoContent();
    }
}
