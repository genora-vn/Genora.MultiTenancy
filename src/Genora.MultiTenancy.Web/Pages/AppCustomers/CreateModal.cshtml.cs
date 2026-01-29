using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.MasterData;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCustomers;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppCustomerDto Customer { get; set; }
    public List<SelectListItem> GenderItems { get; set; }
    public List<SelectListItem> CustomerTypeItems { get; set; }

    private readonly IAppCustomerService _customerService;
    private readonly IAppCustomerTypeService _customerTypeService;
    private readonly IProvinceLookupAppService _provinceLookup;

    public List<SelectListItem> ProvinceItems { get; set; } = new();

    public CreateModalModel(
        IAppCustomerService customerService,
        IAppCustomerTypeService customerTypeService,
        IProvinceLookupAppService provinceLookup)
    {
        _customerService = customerService;
        _customerTypeService = customerTypeService;
        _provinceLookup = provinceLookup;
    }

    public async Task OnGetAsync()
    {
        Customer = new CreateUpdateAppCustomerDto
        {
            IsActive = true
        };

        Customer.CustomerCode = await _customerService.GenerateCustomerCodeAsync();

        BuildGenderItems(selectedGender: null);

        await LoadCustomerTypesAsync();

        var provinces = await _provinceLookup.GetProvincesAsync();
        ProvinceItems = provinces
            .Select(p => new SelectListItem(p.Name, p.Code))
            .ToList();

        ProvinceItems.Insert(0, new SelectListItem("-- Chọn tỉnh/thành --", ""));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            BuildGenderItems(Customer.Gender);
            await LoadCustomerTypesAsync();
            return Page();
        }

        await _customerService.CreateAsync(Customer);
        return NoContent();
    }

    private void BuildGenderItems(byte? selectedGender)
    {
        GenderItems = new List<SelectListItem>
        {
            new SelectListItem("Nam",  "1", selectedGender == 1),
            new SelectListItem("Nữ",   "2", selectedGender == 2),
            new SelectListItem("Khác", "3", selectedGender == 3)
        };
    }

    private async Task LoadCustomerTypesAsync()
    {
        var result = await _customerTypeService.GetListAsync(
            new PagedAndSortedResultRequestDto
            {
                MaxResultCount = 1000,
                Sorting = ""
            }
        );

        CustomerTypeItems = result.Items
            .Select(x => new SelectListItem(
                text: $"{x.Name}",
                value: x.Id.ToString()
            ))
            .ToList();
    }
}