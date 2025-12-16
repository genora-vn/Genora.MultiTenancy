using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
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

    public CreateModalModel(
        IAppCustomerService customerService,
        IAppCustomerTypeService customerTypeService)
    {
        _customerService = customerService;
        _customerTypeService = customerTypeService;
    }

    public async Task OnGetAsync()
    {
        // Khởi tạo DTO
        Customer = new CreateUpdateAppCustomerDto
        {
            IsActive = true
        };

        // Generate mã khách hàng tự động
        Customer.CustomerCode = await _customerService.GenerateCustomerCodeAsync();

        // Load danh sách giới tính
        BuildGenderItems(selectedGender: null);

        // Load danh sách loại khách hàng
        await LoadCustomerTypesAsync();
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
                Sorting = "" // nếu AppCustomerType có DisplayOrder
            }
        );

        CustomerTypeItems = result.Items
            .Select(x => new SelectListItem(
                text: $"{x.Name}",      // hoặc $"{x.Name} ({x.Code})"
                value: x.Id.ToString()
            ))
            .ToList();
    }
}