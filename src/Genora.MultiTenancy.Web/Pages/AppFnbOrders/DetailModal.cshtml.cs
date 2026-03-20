using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbOrders;

public class DetailModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public FnbOrderDetailDto Order { get; set; } = new();

    private readonly IAppFnbOrderService _service;

    public DetailModalModel(IAppFnbOrderService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        Order = await _service.GetAsync(Id);
    }
}