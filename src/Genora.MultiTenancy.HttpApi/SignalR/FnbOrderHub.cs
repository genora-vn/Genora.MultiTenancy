using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.SignalR;

[Authorize]
public class FnbOrderHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var tenantId = CurrentTenantId();
        var group = tenantId.HasValue ? $"fnb-orders:{tenantId}" : "fnb-orders:host";
        await Groups.AddToGroupAsync(Context.ConnectionId, group);
        await base.OnConnectedAsync();
    }

    private Guid? CurrentTenantId()
    {
        var tenantClaim = Context.User?.FindFirst("tenantid")?.Value;
        return Guid.TryParse(tenantClaim, out var tenantId) ? tenantId : null;
    }
}