using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.Tenants;

public class TenantPatchAppService : ApplicationService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IDistributedCache<TenantConfigurationCacheItem> _cache;
    private readonly IUnitOfWorkManager _uow;

    public TenantPatchAppService(
        ITenantRepository tenantRepo,
        IDistributedCache<TenantConfigurationCacheItem> cache,
        IUnitOfWorkManager uow)
    {
        _tenantRepo = tenantRepo;
        _cache = cache;
        _uow = uow;
    }

    [UnitOfWork]
    public virtual async Task<int> PatchAllAsync()
    {
        // ⭐ Lấy list kèm details (bao gồm ConnectionStrings)
        var tenants = await _tenantRepo.GetListAsync(includeDetails: true);
        var patched = 0;

        foreach (var t in tenants)
        {
            var cs = t.GetProperty<string>(Constant.ConnectionString);
            if (!cs.IsNullOrWhiteSpace())
            {
                // Reload 1 tenant kèm details theo Id.
                var tenWithDetails = await _tenantRepo.GetAsync(t.Id, includeDetails: true);

                // ⭐ API 1 tham số (bản ABP mới)
                tenWithDetails.SetDefaultConnectionString(cs);

                await _tenantRepo.UpdateAsync(tenWithDetails, autoSave: true);
                await _cache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenWithDetails.Id));
                await _cache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenWithDetails.Name!));

                patched++;
            }
        }

        await CurrentUnitOfWork.SaveChangesAsync();
        return patched;
    }

    [UnitOfWork]
    public virtual async Task PatchOneAsync(string tenantName)
    {
        if (tenantName.IsNullOrWhiteSpace()) throw new BusinessException("EmptyName");

        // ⭐ Lấy kèm details
        var t = await _tenantRepo.FindByNameAsync(tenantName);
        if (t == null) throw new BusinessException("TenantNotFound");

        var tenWithDetails = await _tenantRepo.GetAsync(t.Id, includeDetails: true);
        var cs = tenWithDetails.GetProperty<string>(Constant.ConnectionString);
        if (cs.IsNullOrWhiteSpace()) throw new BusinessException("TenantConnectionStringEmpty");

        tenWithDetails.SetDefaultConnectionString(cs);
        await _tenantRepo.UpdateAsync(tenWithDetails, autoSave: true);

        await _cache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenWithDetails.Id));
        await _cache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenWithDetails.Name!));
    }
}