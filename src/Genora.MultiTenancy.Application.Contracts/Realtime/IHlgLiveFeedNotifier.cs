using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;

namespace Genora.MultiTenancy.Realtime;

/// <summary>
/// Broadcast hoạt động người chơi cho live-feed Gamification (BD-4).
/// Group theo gameId (client join game đang xem). Mini app anonymous nên không group theo tenant.
/// </summary>
public interface IHlgLiveFeedNotifier
{
    /// <summary>Bắn 1 hoạt động người chơi tới tất cả client đang xem live-feed của game.</summary>
    Task PlayerActivityAsync(Guid gameId, LivePlayerActivityDto activity);
}
