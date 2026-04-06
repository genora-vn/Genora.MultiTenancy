using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppProOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProOrders;

public class UpdateServiceStatusModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateProOrderServiceStatusDto Input { get; set; } = new();

    /// <summary>Trạng thái gốc khi mở modal — dùng để validate server-side.</summary>
    [BindProperty]
    public ProServiceStatus CurrentServiceStatus { get; set; }

    private readonly IAppProOrderService _service;

    public UpdateServiceStatusModalModel(IAppProOrderService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Input.ServiceStatus  = dto.ServiceStatus;
        Input.InternalNote   = dto.InternalNote;
        CurrentServiceStatus = dto.ServiceStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Guard: giá trị 0 = radio disabled không submit được
        if (Input.ServiceStatus == 0)
        {
            ModelState.AddModelError("ServiceStatusValidation", "Vui lòng chọn trạng thái mới.");
            return Page();
        }

        // Guard: không cho lưu lại đúng trạng thái hiện tại
        if (Input.ServiceStatus == CurrentServiceStatus)
        {
            ModelState.AddModelError("ServiceStatusValidation", "Trạng thái mới phải khác trạng thái hiện tại.");
            return Page();
        }

        if (!ModelState.IsValid)
            return Page();

        await _service.UpdateServiceStatusAsync(Id, Input);
        return NoContent();
    }
}
