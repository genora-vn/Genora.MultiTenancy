using Genora.MultiTenancy.AppDtos.MasterData;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;

namespace Genora.MultiTenancy.AppServices.MasterData;
public class ProvinceLookupAppService : ApplicationService, IProvinceLookupAppService
{
    private const string CacheKey = "masterdata:provinces:v2";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IDistributedCache<ProvinceListCacheItem> _cache;
    private readonly ILogger<ProvinceLookupAppService> _logger;

    public ProvinceLookupAppService(
        IHttpClientFactory httpClientFactory,
        IDistributedCache<ProvinceListCacheItem> cache,
        ILogger<ProvinceLookupAppService> logger)
    {
        _httpClientFactory = httpClientFactory;
        _cache = cache;
        _logger = logger;
    }

    public async Task<List<ProvinceLookupDto>> GetProvincesAsync(bool forceRefresh = false)
    {
        if (!forceRefresh)
        {
            var cached = await _cache.GetAsync(CacheKey);
            if (cached?.Items != null && cached.Items.Count > 0)
                return cached.Items;
        }

        try
        {
            var client = _httpClientFactory.CreateClient("ProvincesApi");

            var raw = await client.GetFromJsonAsync<List<ProvinceApiItem>>("/api/v2/");
            var items = (raw ?? new List<ProvinceApiItem>())
                .Where(x => x != null && !string.IsNullOrWhiteSpace(x.Name))
                .Select(x => new ProvinceLookupDto
                {
                    Code = (x.Code ?? 0).ToString(),
                    Name = x.Name!.Trim()
                })
                .OrderBy(x => x.Name)
                .ToList();

            await _cache.SetAsync(
                CacheKey,
                new ProvinceListCacheItem { Items = items },
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(7)
                });

            return items;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Fetch provinces failed");
            throw new BusinessException(MasterDataErrorCodes.ProvincesFetchFailed)
                .WithData("Endpoint", "https://provinces.open-api.vn/api/v2/");
        }
    }

    [Serializable]
    public class ProvinceListCacheItem
    {
        public List<ProvinceLookupDto> Items { get; set; } = new();
    }

    private class ProvinceApiItem
    {
        [JsonPropertyName("code")]
        public int? Code { get; set; }

        [JsonPropertyName("name")]
        public string? Name { get; set; }
    }
}