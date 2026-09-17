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
    private static string GroupName(Guid gameId) => $"hlg-live-feed:{gameId:D}";

    /// <summary>Client gọi để tham gia live-feed của một game.</summary>
    public Task JoinGame(Guid gameId)
        => Groups.AddToGroupAsync(Context.ConnectionId, GroupName(gameId));

    /// <summary>Client gọi để rời live-feed của một game.</summary>
    public Task LeaveGame(Guid gameId)
        => Groups.RemoveFromGroupAsync(Context.ConnectionId, GroupName(gameId));
}
