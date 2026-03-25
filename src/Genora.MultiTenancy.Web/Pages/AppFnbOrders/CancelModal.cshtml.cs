using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbOrders;

public class CancelModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CancelFnbOrderDto Input { get; set; } = new();

    private readonly IAppFnbOrderService _service;

    public CancelModalModel(IAppFnbOrderService service)
    {
        _service = service;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _service.CancelAsync(Id, Input);
        return NoContent();
    }
}