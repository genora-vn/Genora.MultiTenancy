using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppProOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProOrders;

public class UpdatePaymentStatusModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateProOrderPaymentStatusDto Input { get; set; } = new();

    /// <summary>Trạng thái gốc khi mở modal — dùng để validate server-side.</summary>
    [BindProperty]
    public ProPaymentStatus CurrentPaymentStatus { get; set; }

    private readonly IAppProOrderService _service;

    public UpdatePaymentStatusModalModel(IAppProOrderService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Input.PaymentStatus   = dto.PaymentStatus;
        CurrentPaymentStatus  = dto.PaymentStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Guard: giá trị 0 = radio disabled không submit được
        if (Input.PaymentStatus == 0)
        {
            ModelState.AddModelError("PaymentStatusValidation", "Vui lòng chọn trạng thái thanh toán mới.");
            return Page();
        }

        // Guard: không cho lưu lại đúng trạng thái hiện tại
        if (Input.PaymentStatus == CurrentPaymentStatus)
        {
            ModelState.AddModelError("PaymentStatusValidation", "Trạng thái mới phải khác trạng thái hiện tại.");
            return Page();
        }

        if (!ModelState.IsValid)
            return Page();

        await _service.UpdatePaymentStatusAsync(Id, Input);
        return NoContent();
    }
}
