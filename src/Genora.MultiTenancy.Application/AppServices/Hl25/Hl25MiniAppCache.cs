using System;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Volo.Abp.DependencyInjection;

namespace Genora.MultiTenancy.AppServices.Hl25;

public enum Hl25CacheArea { Config, FrameCampaigns, FrameTemplates, Gifts }

/// <summary>
/// Process-local cache for public catalog DTOs only. Register once per application.
/// Scale-out requires a shared cache AND cross-node invalidation/locking before use.
/// </summary>
public sealed class Hl25MiniAppCache : ISingletonDependency, IDisposable
{
    public static readonly TimeSpan Lifetime = TimeSpan.FromMinutes(20);
    private readonly MemoryCache _cache = new(new MemoryCacheOptions { SizeLimit = 4096 });
    // Bounded locks: arbitrary campaign IDs cannot allocate an unbounded semaphore dictionary.
    private readonly SemaphoreSlim[] _locks = Enumerable.Range(0, 256).Select(_ => new SemaphoreSlim(1, 1)).ToArray();

    private readonly TimeProvider _timeProvider;

    public Hl25MiniAppCache(TimeProvider? timeProvider = null)
    {
        _timeProvider = timeProvider ?? TimeProvider.System;
    }

    public async Task<T> GetAsync<T>(Hl25CacheArea area, Guid? tenantId, Func<Task<T>> factory, Guid? campaignId = null)
    {
        var region = Region(area, tenantId);
        var key = area == Hl25CacheArea.FrameTemplates ? $"{region}:{campaignId?.ToString() ?? "all"}" : region;
        if (TryRead(region, key, out T? value))
            return value!;

        var gate = Gate(region);
        await gate.WaitAsync();
        try
        {
            if (TryRead(region, key, out value))
                return value!;

            var versionKey = VersionKey(region);
            if (!_cache.TryGetValue(versionKey, out Guid version))
            {
                version = Guid.NewGuid();
                _cache.Set(versionKey, version, VersionOptions());
            }

            var result = await factory(); // Never cache failures, repositories, tracked entities or user state.
            var entry = new Entry(version, JsonSerializer.Serialize(result), _timeProvider.GetUtcNow() + Lifetime);
            _cache.Set(key, entry, Options());
            // A fresh DTO on every call prevents callers/URL normalization mutating a shared cache object.
            return JsonSerializer.Deserialize<T>(entry.Json)!;
        }
        finally { gate.Release(); }
    }

    public async Task InvalidateAsync(Guid? tenantId, params Hl25CacheArea[] areas)
    {
        foreach (var area in areas.Distinct())
        {
            var region = Region(area, tenantId);
            var gate = Gate(region);
            await gate.WaitAsync();
            try
            {
                // All variants, including old/new campaign IDs, become unreadable together.
                // Serializing with fills prevents an in-flight old DB read repopulating the cache after invalidation.
                _cache.Set(VersionKey(region), Guid.NewGuid(), VersionOptions());
                _cache.Remove(region);
            }
            finally { gate.Release(); }
        }
    }

    private bool TryRead<T>(string region, string key, out T? value)
    {
        if (_cache.TryGetValue(VersionKey(region), out Guid version)
            && _cache.TryGetValue(key, out Entry? entry) && entry != null && entry.Version == version
            && entry.ExpiresAt > _timeProvider.GetUtcNow())
        {
            value = JsonSerializer.Deserialize<T>(entry.Json);
            return true;
        }
        value = default;
        return false;
    }

    private SemaphoreSlim Gate(string region) => _locks[(uint)StringComparer.Ordinal.GetHashCode(region) % _locks.Length];
    private static MemoryCacheEntryOptions Options() => new() { AbsoluteExpirationRelativeToNow = Lifetime, Size = 1 };
    private static MemoryCacheEntryOptions VersionOptions() => new() { SlidingExpiration = Lifetime + TimeSpan.FromMinutes(10), Size = 1 };
    private static string VersionKey(string region) => region + ":version";
    private static string Region(Hl25CacheArea area, Guid? tenantId) => $"hl25:{area switch
    {
        Hl25CacheArea.Config => "config",
        Hl25CacheArea.FrameCampaigns => "frame-campaigns",
        Hl25CacheArea.FrameTemplates => "frame-templates",
        Hl25CacheArea.Gifts => "gifts",
        _ => throw new ArgumentOutOfRangeException(nameof(area))
    }}:{tenantId?.ToString() ?? "host"}";

    private sealed record Entry(Guid Version, string Json, DateTimeOffset ExpiresAt);

    public void Dispose()
    {
        _cache.Dispose();
        foreach (var gate in _locks) gate.Dispose();
    }
}
