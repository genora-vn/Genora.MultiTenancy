using Microsoft.AspNetCore.Authorization;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices;

/// <summary>
/// CrudAppService +:
/// - Tự map permission Tenant/Host
/// - Check Feature cho Tenant
/// </summary>
public abstract class FeatureProtectedCrudAppService<
    TEntity,
    TEntityDto,
    TKey,
    TGetListInput,
    TCreateUpdateDto>
    : CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateUpdateDto>
    where TEntity : class, IEntity<TKey>
    where TGetListInput : IPagedAndSortedResultRequest
{
    protected readonly ICurrentTenant CurrentTenant;
    protected readonly IFeatureChecker FeatureChecker;

    /// <summary>Feature name cho Tenant. Nếu null/empty thì không check feature.</summary>
    protected abstract string FeatureName { get; }

    /// <summary>Root permission TENANT (ví dụ MultiTenancyPermissions.AppSettings.Default).</summary>
    protected abstract string TenantDefaultPermission { get; }

    /// <summary>Root permission HOST (ví dụ MultiTenancyPermissions.HostAppSettings.Default).</summary>
    protected abstract string HostDefaultPermission { get; }

    protected FeatureProtectedCrudAppService(
        IRepository<TEntity, TKey> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository)
    {
        CurrentTenant = currentTenant;
        FeatureChecker = featureChecker;
    }

    /// <summary>
    /// Mặc định: nếu đang ở Host thì map
    /// (TenantDefault + suffix) → (HostDefault + suffix).
    /// Có thể override nếu cần custom.
    /// </summary>
    protected virtual string MapPermissionForSide(string tenantPermission)
    {
        if (CurrentTenant.IsAvailable)
        {
            // Đang ở Tenant: dùng đúng permission tenant.
            return tenantPermission;
        }

        // Đang ở Host
        if (string.IsNullOrWhiteSpace(TenantDefaultPermission) ||
            string.IsNullOrWhiteSpace(HostDefaultPermission))
        {
            return tenantPermission;
        }

        if (tenantPermission.StartsWith(TenantDefaultPermission))
        {
            var suffix = tenantPermission.Substring(TenantDefaultPermission.Length);
            return HostDefaultPermission + suffix;
        }

        return HostDefaultPermission;
    }

    protected virtual async Task EnsureFeatureAsync()
    {
        // Chỉ Tenant mới bị ràng Feature
        if (!CurrentTenant.IsAvailable)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(FeatureName))
        {
            return;
        }

        if (!await FeatureChecker.IsEnabledAsync(FeatureName))
        {
            throw new AbpAuthorizationException($"Feature '{FeatureName}' is disabled for this tenant.");
        }
    }

    // Override các check policy để:
    // 1) Map permission đúng side
    // 2) Check feature

    protected override async Task CheckGetPolicyAsync()
    {
        await AuthorizationService.CheckAsync(MapPermissionForSide(GetPolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckGetListPolicyAsync()
    {
        await AuthorizationService.CheckAsync(MapPermissionForSide(GetListPolicyName ?? GetPolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckCreatePolicyAsync()
    {
        await AuthorizationService.CheckAsync(MapPermissionForSide(CreatePolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckUpdatePolicyAsync()
    {
        await AuthorizationService.CheckAsync(MapPermissionForSide(UpdatePolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckDeletePolicyAsync()
    {
        await AuthorizationService.CheckAsync(MapPermissionForSide(DeletePolicyName));
        await EnsureFeatureAsync();
    }
}