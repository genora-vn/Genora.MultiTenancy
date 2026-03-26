using Genora.MultiTenancy.Realtime;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.SignalR;
public class FnbOrderRealtimeNotifier : IFnbOrderRealtimeNotifier
{
    private readonly IHubContext<FnbOrderHub> _hubContext;

    public FnbOrderRealtimeNotifier(IHubContext<FnbOrderHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public async Task OrderCreatedAsync(Guid orderId)
    {
        await _hubContext.Clients.All.SendAsync("fnb.order.created", orderId);
    }

    public async Task OrderUpdatedAsync(Guid orderId)
    {
        await _hubContext.Clients.All.SendAsync("fnb.order.updated", orderId);
    }
}