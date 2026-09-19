using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.Realtime;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.SignalR;

/// <summary>
/// Broadcast hoạt động người chơi live-feed Gamification (BD-4).
/// Group theo gameId; client join qua HlgLiveFeedHub.JoinGame.
/// </summary>
public class HlgLiveFeedNotifier : IHlgLiveFeedNotifier
{
    private readonly IHubContext<HlgLiveFeedHub> _hubContext;
    private readonly ILogger<HlgLiveFeedNotifier> _logger;
    private readonly Volo.Abp.MultiTenancy.ICurrentTenant _tenant;

    public HlgLiveFeedNotifier(
        IHubContext<HlgLiveFeedHub> hubContext,
        ILogger<HlgLiveFeedNotifier> logger, Volo.Abp.MultiTenancy.ICurrentTenant tenant)
    {
        _hubContext = hubContext;
        _logger = logger; _tenant=tenant;
    }

    public async Task PlayerActivityAsync(Guid gameId, LivePlayerActivityDto activity)
    {
        var group = HlgLiveFeedHub.GroupName(_tenant.Id,gameId);
        _logger.LogInformation("Broadcast hlg.live-feed.activity game={GameId} user={UserId}", gameId, activity.UserId);
        await _hubContext.Clients.Group(group).SendAsync("hlg.live-feed.activity", activity);
    }
}
