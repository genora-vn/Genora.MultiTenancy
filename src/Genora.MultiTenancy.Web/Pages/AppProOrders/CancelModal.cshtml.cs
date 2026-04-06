using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppProOrders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProOrders;

public class CancelModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CancelProOrderDto Input { get; set; } = new();

    private readonly IAppProOrderService _service;

    public CancelModalModel(IAppProOrderService service)
    {
        _service = service;
    }

    public void OnGet() { }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
            return Page();

        await _service.CancelAsync(Id, Input);
        return NoContent();
    }
}
