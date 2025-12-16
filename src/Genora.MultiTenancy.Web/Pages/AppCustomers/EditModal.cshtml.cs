using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.Web.Pages.AppCustomers;

public class EditModalModel : MultiTenancyPageModel
{
    [HiddenInput]
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppCustomerDto Customer { get; set; }
    public List<SelectListItem> GenderItems { get; set; }
    public List<SelectListItem> CustomerTypeItems { get; set; }

    private readonly IAppCustomerService _customerService;
    private readonly IAppCustomerTypeService _customerTypeService;

    public EditModalModel(
        IAppCustomerService customerService,
        IAppCustomerTypeService customerTypeService)
    {
        _customerService = customerService;
        _customerTypeService = customerTypeService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _customerService.GetAsync(Id);
        Customer = ObjectMapper.Map<AppCustomerDto, CreateUpdateAppCustomerDto>(dto);

        BuildGenderItems(Customer.Gender);
        await LoadCustomerTypesAsync(selectedId: Customer.CustomerTypeId);
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            BuildGenderItems(Customer.Gender);
            await LoadCustomerTypesAsync(selectedId: Customer.CustomerTypeId);
            return Page();
        }

        await _customerService.UpdateAsync(Id, Customer);
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

    private async Task LoadCustomerTypesAsync(Guid? selectedId)
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
                value: x.Id.ToString(),
                selected: selectedId.HasValue && x.Id == selectedId.Value
            ))
            .ToList();
    }
}