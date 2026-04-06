using Genora.MultiTenancy.AppDtos.AppProOrders;
using Genora.MultiTenancy.AppServices.AppProOrders;
using Microsoft.AspNetCore.Authorization;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppProOrders.Board;

[Authorize]
public class IndexModel : MultiTenancyPageModel
{
    private readonly IAppProOrderService _proOrderService;

    public List<ProBoardItemDto> Orders { get; private set; } = new();

    public IndexModel(IAppProOrderService proOrderService)
    {
        _proOrderService = proOrderService;
    }

    public async Task OnGetAsync()
    {
        Orders = await _proOrderService.GetBoardAsync(new GetProBoardInput());
    }
}
