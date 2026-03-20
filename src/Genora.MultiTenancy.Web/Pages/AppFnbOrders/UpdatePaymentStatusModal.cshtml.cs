using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbOrders;

public class UpdatePaymentStatusModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateFnbOrderPaymentStatusDto Input { get; set; } = new();

    private readonly IAppFnbOrderService _service;

    public UpdatePaymentStatusModalModel(IAppFnbOrderService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Input.PaymentStatus = dto.PaymentStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdatePaymentStatusAsync(Id, Input);
        return NoContent();
    }
}