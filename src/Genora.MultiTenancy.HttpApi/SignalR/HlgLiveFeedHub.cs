using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.SignalR;

/// <summary>
/// Hub live-feed cho mini app Hoa Linh Gamification (BD-4).
/// Anonymous (mini app không có auth token). Client join group theo gameId để nhận hoạt động người chơi.
/// </summary>
[AllowAnonymous]
public class HlgLiveFeedHub : Hub
{
    private readonly Volo.Abp.MultiTenancy.ICurrentTenant _tenant;
    private readonly Genora.MultiTenancy.AppDtos.Hlg.IHlgGameAppService _games;
    public HlgLiveFeedHub(Volo.Abp.MultiTenancy.ICurrentTenant tenant, Genora.MultiTenancy.AppDtos.Hlg.IHlgGameAppService games) { _tenant=tenant; _games=games; }
    public static string GroupName(Guid? tenantId, Guid gameId) => $"hlg-live-feed:{tenantId?.ToString("D") ?? "host"}:{gameId:D}";

    /// <summary>Client gọi để tham gia live-feed của một game.</summary>
    public async Task JoinGame(Guid gameId)
    { await _games.GetGameAsync(gameId); await Groups.AddToGroupAsync(Context.ConnectionId, GroupName(_tenant.Id,gameId)); }

    /// <summary>Client gọi để rời live-feed của một game.</summary>
    public Task LeaveGame(Guid gameId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(_tenant.Id,gameId));
}
