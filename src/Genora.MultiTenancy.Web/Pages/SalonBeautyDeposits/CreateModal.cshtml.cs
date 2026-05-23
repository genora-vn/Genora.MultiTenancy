using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyDeposits;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateSalonBeautyDepositDto Deposit { get; set; } = new() { PaymentMethod = 1 };

    public List<SelectListItem> CustomerItems { get; set; } = new();

    private readonly ISalonBeautyDepositAppService _depositAppService;
    private readonly ISalonBeautyCustomerAppService _customerAppService;

    public CreateModalModel(
        ISalonBeautyDepositAppService depositAppService,
        ISalonBeautyCustomerAppService customerAppService)
    {
        _depositAppService = depositAppService;
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

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _depositAppService.CreateAsync(Deposit);
        return NoContent();
    }
}
