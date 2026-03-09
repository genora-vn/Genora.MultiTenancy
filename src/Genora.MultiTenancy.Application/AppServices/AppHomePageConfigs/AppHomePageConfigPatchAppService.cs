using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.DomainModels.AppHomePageConfigs;
using Volo.Abp.Application.Services;
using Volo.Abp.Data;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.AppHomePageConfigs;

public class AppHomePageConfigPatchAppService : ApplicationService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly IDataSeeder _dataSeeder;
    private readonly ICurrentTenant _currentTenant;
    private readonly IDataFilter _dataFilter;

    private readonly IRepository<AppHomePageConfig, Guid> _configRepo;

    public AppHomePageConfigPatchAppService(
        ITenantRepository tenantRepo,
        IDataSeeder dataSeeder,
        ICurrentTenant currentTenant,
        IDataFilter dataFilter,
        IRepository<AppHomePageConfig, Guid> configRepo)
    {
        _tenantRepo = tenantRepo;
        _dataSeeder = dataSeeder;
        _currentTenant = currentTenant;
        _dataFilter = dataFilter;
        _configRepo = configRepo;
    }

    /// <summary>
    /// Backfill HomePageConfig cho các tenant CHƯA có dữ liệu.
    /// </summary>
    [UnitOfWork]
    public virtual async Task<int> SeedMissingForAllTenantsAsync()
    {
        // chạy host context để list tenants
        using (_currentTenant.Change(null))
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var tenants = await _tenantRepo.GetListAsync();
            var patched = 0;

            foreach (var t in tenants)
            {
                // check tenant đã có config chưa
                using (_currentTenant.Change(t.Id, t.Name))
                {
                    var hasConfig = await _configRepo.AnyAsync(x => x.TenantId == t.Id);
                    if (hasConfig) continue;

                    // chạy contributor (sẽ clone host template sang tenant)
                    await _dataSeeder.SeedAsync(new DataSeedContext(t.Id));
                    patched++;
                }
            }

            return patched;
        }
    }
}