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

    public HlgLiveFeedNotifier(
        IHubContext<HlgLiveFeedHub> hubContext,
        ILogger<HlgLiveFeedNotifier> logger)
    {
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task PlayerActivityAsync(Guid gameId, LivePlayerActivityDto activity)
    {
        var group = $"hlg-live-feed:{gameId:D}";
        _logger.LogInformation("Broadcast hlg.live-feed.activity game={GameId} user={UserId}", gameId, activity.UserId);
        await _hubContext.Clients.Group(group).SendAsync("hlg.live-feed.activity", activity);
    }
}
