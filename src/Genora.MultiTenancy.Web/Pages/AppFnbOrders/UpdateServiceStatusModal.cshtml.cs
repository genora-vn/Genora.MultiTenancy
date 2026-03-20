using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbOrders;

public class UpdateServiceStatusModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public UpdateFnbOrderServiceStatusDto Input { get; set; } = new();

    private readonly IAppFnbOrderService _service;

    public UpdateServiceStatusModalModel(IAppFnbOrderService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);
        Input.ServiceStatus = dto.ServiceStatus;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await _service.UpdateServiceStatusAsync(Id, Input);
        return NoContent();
    }
}