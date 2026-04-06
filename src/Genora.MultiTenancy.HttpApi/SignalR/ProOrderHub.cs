using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Security.Claims;

namespace Genora.MultiTenancy.SignalR;

[Authorize]
public class ProOrderHub : Hub
{
    private const string HostGroup = "pro-orders:host";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ResolveGroupName());
        await base.OnConnectedAsync();
    }

    public Task PingBell()
    {
        return Clients.Caller.SendAsync("pro.ping");
    }

    private string ResolveGroupName()
    {
        var tenantClaim =
            Context.User?.FindFirstValue(AbpClaimTypes.TenantId) ??
            Context.User?.FindFirstValue("tenantid");

        return Guid.TryParse(tenantClaim, out var tenantId)
            ? $"pro-orders:{tenantId:D}"
            : HostGroup;
    }
}
