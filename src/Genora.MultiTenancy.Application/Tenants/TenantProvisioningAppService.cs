using Genora.MultiTenancy.Data;
using System;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.Tenants;

public class TenantProvisioningAppService : ApplicationService, ITenantProvisioningAppService
{
    private readonly ITenantRepository _tenantRepo;
    private readonly ITenantManager _tenantManager;
    private readonly IDistributedCache<TenantConfigurationCacheItem> _tenantCache;
    private readonly ICurrentTenant _currentTenant;
    private readonly IMultiTenancyDbSchemaMigrator _migrator; // ⬅️ abstraction (không EF ở Application)
    private readonly IUnitOfWorkManager _uow;
    private readonly IDataSeeder _dataSeeder;          // chạy tất cả contributor Domain (Books…)
    private readonly IIdentityDataSeeder _identitySeeder; // seed admin user/role

    public TenantProvisioningAppService(
        ITenantRepository tenantRepo,
        ITenantManager tenantManager,
        IDistributedCache<TenantConfigurationCacheItem> tenantCache,
        ICurrentTenant currentTenant,
        IMultiTenancyDbSchemaMigrator migrator,
        IUnitOfWorkManager uow,
        IDataSeeder dataSeeder,
        IIdentityDataSeeder identitySeeder)
    {
        _tenantRepo = tenantRepo;
        _tenantManager = tenantManager;
        _tenantCache = tenantCache;
        _currentTenant = currentTenant;
        _migrator = migrator;
        _uow = uow;
        _dataSeeder = dataSeeder;
        _identitySeeder = identitySeeder;
    }

    [UnitOfWork]
    public virtual async Task<Guid> CreateAndProvisionAsync(CreateTenantProvisionDto input, CancellationToken cancellationToken = default)
    {
        if (input.Name.IsNullOrWhiteSpace()) throw new BusinessException("TenantNameRequired");
        if (input.Host.IsNullOrWhiteSpace()) throw new BusinessException("TenantHostRequired");
        if (input.ConnectionString.IsNullOrWhiteSpace()) throw new BusinessException("TenantConnectionRequired");

        // 1) Tạo tenant trên Host DB
        if (await _tenantRepo.FindByNameAsync(input.Name) != null)
            throw new BusinessException("TenantAlreadyExists");

        var tenant = await _tenantManager.CreateAsync(input.Name);

        tenant.SetProperty(Constant.Host, input.Host);
        tenant.SetProperty(Constant.ConnectionString, input.ConnectionString);
        tenant.SetProperty(Constant.IsActive, input.IsActive);

        // ABP (API mới): set default connection (ghi vào AbpTenantConnectionStrings)
        tenant.SetDefaultConnectionString(input.ConnectionString);

        await _tenantRepo.InsertAsync(tenant, autoSave: true);
        await CurrentUnitOfWork.SaveChangesAsync();

        // Xoá cache TenantStore để dùng config mới ngay
        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Id));
        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Name!));

        // 2) Migrate + seed trong NGỮ CẢNH TENANT (DB riêng)
        using (_currentTenant.Change(tenant.Id, tenant.Name))
        using (var uow = _uow.Begin(requiresNew: true, isTransactional: false))
        {
            await _migrator.MigrateAsync(); // EFCore implement sẽ migrate DB tenant hiện tại

            // seed admin (username=admin, email=AdminEmail)
            await _identitySeeder.SeedAsync(input.AdminEmail, input.AdminPassword, tenant.Id);

            // chạy tất cả IDataSeedContributor (ví dụ Books) trong DB tenant
            await _dataSeeder.SeedAsync(new DataSeedContext(tenant.Id));
            await uow.CompleteAsync();
        }

        return tenant.Id;
    }
}