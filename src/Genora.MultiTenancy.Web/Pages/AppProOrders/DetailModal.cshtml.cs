using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppProOrders;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProOrders;

public class DetailModalModel : MultiTenancyPageModel
{
    [Microsoft.AspNetCore.Mvc.BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public ProOrderDetailDto Order { get; set; } = new();

    private readonly IAppProOrderService _service;

    public DetailModalModel(IAppProOrderService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        Order = await _service.GetAsync(Id);
    }
}
