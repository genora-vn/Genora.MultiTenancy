using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hlg;

/// <summary>
/// Xếp hạng Gamification (BD-5). Ranking reset theo sự kiện:
/// điểm tính từ các phiên game đã finish trong khoảng [StartAt, EndAt] của sự kiện hiện tại.
/// Internal service — controller gọi trực tiếp.
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlgRankingAppService : ApplicationService, IHlgRankingAppService
{
    private readonly IRepository<HlgRankingEvent, Guid> _eventRepo;
    private readonly IRepository<HlgGameSession, Guid> _sessionRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ILogger<HlgRankingAppService> _logger;

    public HlgRankingAppService(
        IRepository<HlgRankingEvent, Guid> eventRepo,
        IRepository<HlgGameSession, Guid> sessionRepo,
        IRepository<Customer, Guid> customerRepo,
        ILogger<HlgRankingAppService> logger)
    {
        _eventRepo = eventRepo;
        _sessionRepo = sessionRepo;
        _customerRepo = customerRepo;
        _logger = logger;
    }

    public async Task<RankingEventDto?> GetCurrentEventAsync(CancellationToken ct = default)
    {
        var ev = await GetActiveEventAsync(ct);
        return ev == null ? null : MapEvent(ev);
    }

    public async Task<List<RankingEntryDto>> GetEntriesAsync(string? phone = null, int top = 50, CancellationToken ct = default)
    {
        var ev = await GetActiveEventAsync(ct);
        if (ev == null) return new List<RankingEntryDto>();

        return await BuildEntriesAsync(ev, phone, top, ct);
    }
    public async Task<List<RankingEntryDto>> GetEventEntriesAsync(Guid eventId, string? phone = null, int top = 50, CancellationToken ct = default)
    {
        var ev = await AsyncExecuter.FirstOrDefaultAsync((await _eventRepo.GetQueryableAsync()).Where(x => x.Id == eventId && x.IsActive && x.TenantId == CurrentTenant.Id));
        if (ev == null) return new();
        return await BuildEntriesAsync(ev, phone, top, ct);
    }
    private async Task<List<RankingEntryDto>> BuildEntriesAsync(HlgRankingEvent ev, string? phone, int top, CancellationToken ct)
    {
        top = Math.Clamp(top, 1, 1000);
        // Tổng điểm mỗi người chơi = sum(Score) các phiên finish trong khoảng sự kiện (BD-5).
        var sessionQ = await _sessionRepo.GetQueryableAsync();
        var finished = sessionQ.Where(s =>
            s.TenantId == CurrentTenant.Id && (ev.GameId == null || s.GameId == ev.GameId) && s.IsFinished
            && s.FinishedAt != null
            && s.FinishedAt >= ev.StartAt
            && s.FinishedAt <= ev.EndAt);

        var aggregated = await AsyncExecuter.ToListAsync(
            finished.GroupBy(s => s.CustomerId)
                    .Select(g => new { CustomerId = g.Key, Score = g.Sum(x => x.Score) }), ct);

        if (aggregated.Count == 0) return new List<RankingEntryDto>();

        // Xác định customer hiện tại (nếu có phone).
        Guid? currentCustomerId = null;
        var normalized = NormalizePhone(phone);
        if (!string.IsNullOrWhiteSpace(normalized))
        {
            var current = await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct);
            currentCustomerId = current?.Id;
        }

        // Sắp xếp giảm dần theo điểm → gán rank (tuple, tránh dynamic/anonymous-type footgun).
        var ranked = aggregated
            .OrderByDescending(x => x.Score).ThenBy(x => x.CustomerId)
            .Select((x, i) => (CustomerId: x.CustomerId, Score: x.Score, Rank: i + 1))
            .ToList();

        // Lấy thông tin hiển thị của các customer liên quan (top + current).
        var neededIds = ranked.Take(top).Select(x => x.CustomerId).ToList();
        if (currentCustomerId.HasValue && !neededIds.Contains(currentCustomerId.Value))
            neededIds.Add(currentCustomerId.Value);

        var custQ = await _customerRepo.GetQueryableAsync();
        var customers = await AsyncExecuter.ToListAsync(
            custQ.Where(c => neededIds.Contains(c.Id))
                 .Select(c => new { c.Id, c.FullName, c.AvatarUrl }), ct);
        var custById = customers.ToDictionary(x => x.Id, x => x);

        RankingEntryDto ToDto((Guid CustomerId, int Score, int Rank) r)
        {
            custById.TryGetValue(r.CustomerId, out var c);
            return new RankingEntryDto
            {
                Rank = r.Rank,
                UserId = r.CustomerId,
                DisplayName = c?.FullName ?? "Người chơi",
                AvatarUrl = c?.AvatarUrl,
                Score = r.Score,
                IsCurrentUser = currentCustomerId.HasValue && r.CustomerId == currentCustomerId.Value
            };
        }

        var result = ranked.Take(top).Select(ToDto).ToList();

        // Luôn kèm dòng của user hiện tại nếu ngoài top (để mini app hiển thị vị trí).
        if (currentCustomerId.HasValue && result.All(e => e.UserId != currentCustomerId.Value))
        {
            var mine = ranked.FirstOrDefault(x => x.CustomerId == currentCustomerId.Value);
            if (mine.CustomerId != Guid.Empty) result.Add(ToDto(mine));
        }

        return result;
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    /// <summary>Sự kiện đang hiệu lực: IsActive + đang trong khoảng thời gian; ưu tiên mới nhất.</summary>
    private async Task<HlgRankingEvent?> GetActiveEventAsync(CancellationToken ct)
    {
        var now = Clock.Now;
        var q = await _eventRepo.GetQueryableAsync();
        var events = await AsyncExecuter.ToListAsync(
            q.Where(e => e.TenantId == CurrentTenant.Id && e.IsActive && e.StartAt <= now && e.EndAt >= now)
             .OrderByDescending(e => e.StartAt), ct);

        return events.FirstOrDefault();
    }

    private static RankingEventDto MapEvent(HlgRankingEvent e) => new()
    {
        Id = e.Id,
        GameId = e.GameId,
        Title = e.Title,
        Description = e.Description,
        StartAt = e.StartAt,
        EndAt = e.EndAt
    };

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return Regex.Replace(phone.Trim(), @"\s+|-|\.", "");
    }
}
