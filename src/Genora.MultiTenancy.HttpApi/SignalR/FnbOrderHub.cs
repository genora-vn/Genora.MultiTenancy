using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Volo.Abp.Security.Claims;

namespace Genora.MultiTenancy.SignalR;

[Authorize]
public class FnbOrderHub : Hub
{
    private const string HostGroup = "fnb-orders:host";

    public override async Task OnConnectedAsync()
    {
        await Groups.AddToGroupAsync(Context.ConnectionId, ResolveGroupName());
        await base.OnConnectedAsync();
    }

    public Task PingBell()
    {
        return Clients.Caller.SendAsync("fnb.ping");
    }

    private string ResolveGroupName()
    {
        var tenantClaim =
            Context.User?.FindFirstValue(AbpClaimTypes.TenantId) ??
            Context.User?.FindFirstValue("tenantid");

        return Guid.TryParse(tenantClaim, out var tenantId)
            ? $"fnb-orders:{tenantId:D}"
            : HostGroup;
    }
}