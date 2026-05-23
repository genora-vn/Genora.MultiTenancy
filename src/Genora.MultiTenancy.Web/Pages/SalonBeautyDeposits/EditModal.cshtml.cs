using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyDeposits;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateSalonBeautyDepositDto Deposit { get; set; } = new();

    public Guid CustomerId { get; set; }
    public string? CustomerLabel { get; set; }
    public string? TransactionCode { get; set; }
    public List<SelectListItem> CustomerItems { get; set; } = new();

    private readonly ISalonBeautyDepositAppService _depositAppService;
    private readonly ISalonBeautyCustomerAppService _customerAppService;

    public EditModalModel(
        ISalonBeautyDepositAppService depositAppService,
        ISalonBeautyCustomerAppService customerAppService)
    {
        _depositAppService = depositAppService;
        _customerAppService = customerAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        var dto = await _depositAppService.GetAsync(id);
        Id = id;
        TransactionCode = dto.TransactionCode;
        CustomerId = dto.CustomerId;
        CustomerLabel = $"{dto.CustomerName} - {dto.CustomerPhone}";

        Deposit = new UpdateSalonBeautyDepositDto
        {
            Amount = dto.Amount,
            PaymentMethod = dto.PaymentMethod,
            ReferenceCode = dto.ReferenceCode,
            Note = dto.Note
        };

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

        await _depositAppService.UpdateAsync(Id, Deposit);
        return NoContent();
    }
}
