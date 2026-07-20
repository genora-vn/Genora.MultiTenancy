using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.MasterData;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Localization;
using System;
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
    public List<SelectListItem> CustomerSourceItems { get; set; } = new();

    private readonly IAppCustomerService _customerService;
    private readonly IMiniAppCustomerTypeService _customerTypeService;
    private readonly IProvinceLookupAppService _provinceLookup;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public List<SelectListItem> ProvinceItems { get; set; } = new();

    public CreateModalModel(
        IAppCustomerService customerService,
        IMiniAppCustomerTypeService customerTypeService,
        IProvinceLookupAppService provinceLookup,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _customerService = customerService;
        _customerTypeService = customerTypeService;
        _provinceLookup = provinceLookup;
        _l = l;
    }

    public async Task OnGetAsync()
    {
        Customer = new CreateUpdateAppCustomerDto
        {
            IsActive = true,
            CustomerSource = CustomerSource.Manual
        };

        Customer.CustomerCode = await _customerService.GenerateCustomerCodeAsync();

        BuildGenderItems(selectedGender: null);
        BuildCustomerSourceItems();

        await LoadCustomerTypesAsync();

        var provinces = await _provinceLookup.GetProvincesAsync();
        ProvinceItems = provinces
            .Select(p => new SelectListItem(p.Name, p.Code))
            .ToList();

        ProvinceItems.Insert(0, new SelectListItem($"-- {_l["Customer:ProvinceCodePlaceholder"]} --", ""));
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            BuildGenderItems(Customer.Gender);
            BuildCustomerSourceItems();
            await LoadCustomerTypesAsync();
            return Page();
        }

        // Thủ công từ CMS luôn là Manual (không cho phép thay đổi từ UI)
        Customer.CustomerSource = CustomerSource.Manual;

        await _customerService.CreateAsync(Customer);
        return NoContent();
    }

    private void BuildCustomerSourceItems()
    {
        CustomerSourceItems = Enum.GetValues<CustomerSource>()
            .Select(s => new SelectListItem(
                _l[$"CustomerSource:{s}"],
                ((int)s).ToString(),
                Customer != null && Customer.CustomerSource == s))
            .ToList();
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