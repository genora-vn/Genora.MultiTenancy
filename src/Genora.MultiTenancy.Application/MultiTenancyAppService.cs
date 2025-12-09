using Genora.MultiTenancy.Localization;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;

namespace Genora.MultiTenancy;

/* Inherit your application services from this class.
 */
public abstract class MultiTenancyAppService : ApplicationService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IDistributedCache<TenantConfigurationCacheItem> _cache; // cache của AbpTenantStore
    protected MultiTenancyAppService(ITenantRepository tenantRepo, IDistributedCache<TenantConfigurationCacheItem> cache)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        _tenantRepo = tenantRepo;
        _cache = cache;
    }
    public async Task SetDefaultConnectionAsync(string tenantName, string connectionString)
    {
        var t = await _tenantRepo.FindByNameAsync(tenantName)
                ?? throw new BusinessException("TenantNotFound");

        // Lưu ExtraProperties để hiển thị trong UI
        t.SetProperty(Constant.ConnectionString, connectionString);

        // ⭐ mapping về "Default connection" của ABP
        t.SetDefaultConnectionString(connectionString);

        await _tenantRepo.UpdateAsync(t, autoSave: true);

        // ⭐ XÓA CACHE TENANT STORE (nếu không, resolver vẫn lấy bản cũ)
        await _cache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(t.Id));
        await _cache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(t.Name));
    }
}
