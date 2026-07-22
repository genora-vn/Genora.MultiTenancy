using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

/// <summary>
/// Gom logic lấy tin tức Zalo OA cho Mini App + cache per-tenant.
/// Đây là nguồn "Zalo OA" (khác IMiniAppNewsService — tin nội bộ AppNews).
/// </summary>
public class MiniAppZaloNewsService : ApplicationService, IMiniAppZaloNewsService
{
    private readonly IZaloApiClient _zaloApiClient;
    private readonly IDistributedCache<ZaloArticleListResponse> _listCache;
    private readonly IDistributedCache<ZaloArticleDetailResponse> _detailCache;
    private readonly IConfiguration _cfg;
    private readonly ICurrentTenant _currentTenant;
    private readonly ILogger<MiniAppZaloNewsService> _logger;

    // TTL mặc định 5 phút; override qua config Zalo:NewsCacheMinutes.
    private const int DefaultCacheMinutes = 5;

    public MiniAppZaloNewsService(
        IZaloApiClient zaloApiClient,
        IDistributedCache<ZaloArticleListResponse> listCache,
        IDistributedCache<ZaloArticleDetailResponse> detailCache,
        IConfiguration cfg,
        ICurrentTenant currentTenant,
        ILogger<MiniAppZaloNewsService> logger)
    {
        _zaloApiClient = zaloApiClient;
        _listCache = listCache;
        _detailCache = detailCache;
        _cfg = cfg;
        _currentTenant = currentTenant;
        _logger = logger;
    }

    public async Task<ZaloArticleListResponse> GetArticleListAsync(int offset, int limit, string type, CancellationToken ct = default)
    {
        var normalizedType = string.IsNullOrWhiteSpace(type) ? "normal" : type.Trim();
        var cacheKey = $"zalo-news:list:{TenantKey()}:{normalizedType}:{offset}:{limit}";

        var cached = await SafeGetAsync(() => _listCache.GetAsync(cacheKey, token: ct));
        if (cached != null)
            return cached;

        var result = await _zaloApiClient.GetArticleListAsync(offset, limit, normalizedType, ct);

        // Chỉ cache khi Zalo trả thành công (error==0) — tránh cache lỗi/token hết hạn.
        if (result != null && result.Error == 0)
            await SafeSetAsync(() => _listCache.SetAsync(cacheKey, result, CacheOptions(), token: ct));

        return result ?? new ZaloArticleListResponse { Error = -1, Message = "Không lấy được danh sách tin tức" };
    }

    public async Task<ZaloArticleDetailResponse> GetArticleDetailAsync(string articleId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(articleId))
            return new ZaloArticleDetailResponse { Error = -1, Message = "Thiếu mã bài viết" };

        var id = articleId.Trim();
        var cacheKey = $"zalo-news:detail:{TenantKey()}:{id}";

        var cached = await SafeGetAsync(() => _detailCache.GetAsync(cacheKey, token: ct));
        if (cached != null)
            return cached;

        var result = await _zaloApiClient.GetArticleDetailAsync(id, ct);

        if (result != null && result.Error == 0)
            await SafeSetAsync(() => _detailCache.SetAsync(cacheKey, result, CacheOptions(), token: ct));

        return result ?? new ZaloArticleDetailResponse { Error = -1, Message = "Không lấy được chi tiết tin tức" };
    }

    private string TenantKey() => _currentTenant.Id?.ToString() ?? "host";

    private DistributedCacheEntryOptions CacheOptions()
    {
        var minutes = int.TryParse(_cfg["Zalo:NewsCacheMinutes"], out var m) && m > 0
            ? m
            : DefaultCacheMinutes;

        return new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(minutes)
        };
    }

    // Cache lỗi (vd Redis chết) KHÔNG được làm sập luồng đọc tin — fallback gọi thẳng API.
    private async Task<T?> SafeGetAsync<T>(Func<Task<T?>> getter) where T : class
    {
        try { return await getter(); }
        catch (Exception ex)
        {
            _logger.LogException(ex);
            return null;
        }
    }

    private async Task SafeSetAsync(Func<Task> setter)
    {
        try { await setter(); }
        catch (Exception ex) { _logger.LogException(ex); }
    }
}
