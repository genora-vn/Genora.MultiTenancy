using Genora.MultiTenancy.AppDtos.AppFnbOrders;
using Genora.MultiTenancy.AppServices.AppFnbOrders;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppFnbOrders.Kitchen;

public class IndexModel : MultiTenancyPageModel
{
    private readonly IAppFnbOrderService _appFnbOrderService;

    public List<FnbKitchenBoardItemDto> Orders { get; private set; } = new();

    public IndexModel(IAppFnbOrderService appFnbOrderService)
    {
        _appFnbOrderService = appFnbOrderService;
    }

    public async Task OnGetAsync()
    {
        Orders = await _appFnbOrderService.GetKitchenBoardAsync(new GetFnbKitchenBoardInput());
    }
}