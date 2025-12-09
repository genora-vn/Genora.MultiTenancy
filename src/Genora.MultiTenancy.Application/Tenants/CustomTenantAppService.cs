using Genora.MultiTenancy.Data;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Caching;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.TenantManagement;
using Volo.Abp.Uow;
using System.Linq.Dynamic.Core;

namespace Genora.MultiTenancy.Tenants;

[Dependency(ReplaceServices = true)]
[ExposeServices(typeof(ITenantAppService))]
[RemoteService(Name = TenantManagementRemoteServiceConsts.RemoteServiceName)]
public class CustomTenantAppService : ApplicationService, ITenantAppService
{
    private readonly ITenantRepository _tenantRepository;
    private readonly ITenantManager _tenantManager;
    private readonly IDistributedCache<TenantConfigurationCacheItem> _tenantCache;
    private readonly ICurrentTenant _currentTenant;
    private readonly IMultiTenancyDbSchemaMigrator _migrator;
    private readonly IUnitOfWorkManager _uow;
    private readonly IDataSeeder _dataSeeder;
    private readonly IIdentityDataSeeder _identitySeeder;
    private readonly IRepository<Tenant, Guid> _tenantGenericRepository; // ⬅️ generic repo để lấy IQueryable
    private readonly IDataFilter _dataFilter;

    public CustomTenantAppService(
        ITenantRepository tenantRepository,
        ITenantManager tenantManager,
        IDistributedCache<TenantConfigurationCacheItem> tenantCache,
        ICurrentTenant currentTenant,
        IMultiTenancyDbSchemaMigrator migrator,
        IUnitOfWorkManager uow,
        IDataSeeder dataSeeder,
        IIdentityDataSeeder identitySeeder,
        IRepository<Tenant, Guid> tenantGenericRepository,
        IDataFilter dataFilter)
    {
        _tenantRepository = tenantRepository;
        _tenantManager = tenantManager;
        _tenantCache = tenantCache;
        _currentTenant = currentTenant;
        _migrator = migrator;
        _uow = uow;
        _dataSeeder = dataSeeder;
        _identitySeeder = identitySeeder;
        _tenantGenericRepository = tenantGenericRepository;
        _dataFilter = dataFilter;
    }

    // --------------------- CREATE ---------------------
    [Authorize(TenantManagementPermissions.Tenants.Create)]
    [Volo.Abp.Uow.UnitOfWork]
    public async Task<TenantDto> CreateAsync(TenantCreateDto input)
    {
        var host = input.ExtraProperties?.GetOrDefault(Constant.Host) as string;
        var conn = input.ExtraProperties?.GetOrDefault(Constant.ConnectionString) as string;
        var isActive = input.ExtraProperties?.GetOrDefault(Constant.IsActive) as bool? ?? true;

        if (host.IsNullOrWhiteSpace()) throw new BusinessException("HostRequired");
        if (conn.IsNullOrWhiteSpace()) throw new BusinessException("ConnectionRequired");

        Guid tenantId;
        string tenantName;

        // 1) HOST DB: tạo tenant + set default connection
        using (var uow = _uow.Begin(requiresNew: true, isTransactional: false))
        {
            var tenant = await _tenantManager.CreateAsync(input.Name);
            tenant.SetProperty(Constant.Host, host);
            tenant.SetProperty(Constant.ConnectionString, conn);
            tenant.SetProperty(Constant.IsActive, isActive);
            tenant.SetDefaultConnectionString(conn);

            await _tenantRepository.InsertAsync(tenant, autoSave: true);
            await uow.CompleteAsync();

            tenantId = tenant.Id;
            tenantName = tenant.Name!;
        }

        // Clear cache tenant store để Resolver dùng ngay chuỗi mới
        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenantId));
        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenantName));

        // 2) TENANT DB: migrate + seed
        using (_currentTenant.Change(tenantId, tenantName))
        using (var uow = _uow.Begin(requiresNew: true, isTransactional: false))
        {
            await _migrator.MigrateAsync(); // ⬅️ tạo DB nếu thiếu + migrate

            await _identitySeeder.SeedAsync(input.AdminEmailAddress, input.AdminPassword, tenantId);
            await _dataSeeder.SeedAsync(new DataSeedContext(tenantId));
            await uow.CompleteAsync();
        }

        // Trả DTO
        using (_currentTenant.Change(null))
        {
            var entity = await _tenantRepository.GetAsync(tenantId);
            return ObjectMapper.Map<Tenant, TenantDto>(entity);
        }
    }


    // --------------------- UPDATE ---------------------
    [Authorize(TenantManagementPermissions.Tenants.Update)]
    [UnitOfWork]
    public virtual async Task<TenantDto> UpdateAsync(Guid id, TenantUpdateDto input)
    {
        var tenant = await _tenantRepository.GetAsync(id, includeDetails: true);

        await _tenantManager.ChangeNameAsync(tenant, input.Name);

        var host = input.ExtraProperties?.GetOrDefault(Constant.Host) as string;
        var conn = input.ExtraProperties?.GetOrDefault(Constant.ConnectionString) as string;
        var isActive = input.ExtraProperties?.GetOrDefault(Constant.IsActive) as bool?;

        if (!host.IsNullOrWhiteSpace()) tenant.SetProperty(Constant.Host, host);
        if (isActive.HasValue) tenant.SetProperty(Constant.IsActive, isActive.Value);

        if (!conn.IsNullOrWhiteSpace())
        {
            tenant.SetProperty(Constant.ConnectionString, conn);
            tenant.SetDefaultConnectionString(conn); // đồng bộ Default
        }

        await _tenantRepository.UpdateAsync(tenant, autoSave: true);
        await CurrentUnitOfWork.SaveChangesAsync();

        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Id));
        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Name!));

        return ObjectMapper.Map<Tenant, TenantDto>(tenant);
    }

    // --------------------- DELETE ---------------------
    [Authorize(TenantManagementPermissions.Tenants.Delete)]
    public virtual Task DeleteAsync(Guid id) => _tenantRepository.DeleteAsync(id);

    // --------------------- GET (single) ---------------------
    [Authorize(TenantManagementPermissions.Tenants.Default)]
    public async Task<TenantDto> GetAsync(Guid id)
    {
        using (_currentTenant.Change(null))
        using (_dataFilter.Disable<IMultiTenant>())
        {
            var entity = await _tenantGenericRepository.GetAsync(id);
            return ObjectMapper.Map<Tenant, TenantDto>(entity);
        }
    }

    // --------------------- LIST ---------------------
    [Authorize(TenantManagementPermissions.Tenants.Default)]
    public async Task<PagedResultDto<TenantDto>> GetListAsync(GetTenantsInput input)
    {
        using (_currentTenant.Change(null))                 // ⭐ Host context
        using (_dataFilter.Disable<IMultiTenant>())         // ⭐ tắt filter đa tenant (an toàn tuyệt đối)
        {
            var query = await _tenantGenericRepository.GetQueryableAsync();

            // Filter theo tên (nếu có)
            if (!input.Filter.IsNullOrWhiteSpace())
            {
                query = query.Where(t => t.Name.Contains(input.Filter));
            }

            // Sorting an toàn với Dynamic LINQ
            var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? nameof(Tenant.Name) : input.Sorting;
            if (sorting.StartsWith("name", StringComparison.OrdinalIgnoreCase))
            {
                sorting = "Name" + sorting.Substring("name".Length); // "name asc" -> "Name asc"
            }

            var total = await AsyncExecuter.CountAsync(query);

            var list = await AsyncExecuter.ToListAsync(
                query
                  .OrderBy(sorting)
                  .Skip(input.SkipCount)
                  .Take(input.MaxResultCount)
            );

            var items = ObjectMapper.Map<List<Tenant>, List<TenantDto>>(list);
            return new PagedResultDto<TenantDto>(total, items);
        }
    }

    // --------------------- DEFAULT CONNECTION STRING (bổ sung cho ITenantAppService) ---------------------
    // Xem connection string mặc định
    [Authorize(TenantManagementPermissions.Tenants.Default)]
    public virtual async Task<string> GetDefaultConnectionStringAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetAsync(id, includeDetails: true);
        // ABP mới: FindDefaultConnectionString() trả null nếu chưa set
        return tenant.FindDefaultConnectionString();
    }

    // Cập nhật connection string mặc định
    [Authorize(TenantManagementPermissions.Tenants.ManageConnectionStrings)] // nếu không có, dùng .Update
    [UnitOfWork]
    public virtual async Task UpdateDefaultConnectionStringAsync(Guid id, string defaultConnectionString)
    {
        var tenant = await _tenantRepository.GetAsync(id, includeDetails: true);
        tenant.SetProperty(Constant.ConnectionString, defaultConnectionString); // để UI thấy
        tenant.SetDefaultConnectionString(defaultConnectionString);             // để resolver dùng
        await _tenantRepository.UpdateAsync(tenant, autoSave: true);
        await CurrentUnitOfWork.SaveChangesAsync();

        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Id));
        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Name!));
    }

    // Xoá connection string mặc định
    [Authorize(TenantManagementPermissions.Tenants.ManageConnectionStrings)] // nếu không có, dùng .Update
    [UnitOfWork]
    public virtual async Task DeleteDefaultConnectionStringAsync(Guid id)
    {
        var tenant = await _tenantRepository.GetAsync(id, includeDetails: true);
        tenant.RemoveDefaultConnectionString();
        await _tenantRepository.UpdateAsync(tenant, autoSave: true);
        await CurrentUnitOfWork.SaveChangesAsync();

        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Id));
        await _tenantCache.RemoveAsync(TenantConfigurationCacheItem.CalculateCacheKey(tenant.Name!));
    }
}