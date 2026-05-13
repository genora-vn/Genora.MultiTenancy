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
/// CrudAppService Base +:
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

    protected abstract string FeatureName { get; }
    protected abstract string TenantDefaultPermission { get; }
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

    protected virtual string MapPermissionForSide(string tenantPermission)
    {
        if (string.IsNullOrWhiteSpace(tenantPermission))
            throw new AbpAuthorizationException("Missing policy name.");

        if (CurrentTenant.IsAvailable)
            return tenantPermission;

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
        if (!CurrentTenant.IsAvailable) return;
        if (string.IsNullOrWhiteSpace(FeatureName)) return;

        if (!await FeatureChecker.IsEnabledAsync(FeatureName))
            throw new AbpAuthorizationException($"Feature '{FeatureName}' is disabled for this tenant.");
    }

    private async Task CheckPolicyRequiredAsync(string? policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            throw new AbpAuthorizationException("Missing policy name.");

        await AuthorizationService.CheckAsync(policyName);
    }

    protected override async Task CheckGetPolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(GetPolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckGetListPolicyAsync()
    {
        var policy = GetListPolicyName ?? GetPolicyName;
        await CheckPolicyRequiredAsync(MapPermissionForSide(policy));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckCreatePolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(CreatePolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckUpdatePolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(UpdatePolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckDeletePolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(DeletePolicyName));
        await EnsureFeatureAsync();
    }
}

/// <summary>
/// CrudAppService Base cho trường hợp Create DTO và Update DTO tách riêng.
/// Dùng class này để tránh ABP sinh duplicate conventional routes từ inherited CRUD methods.
/// </summary>
public abstract class FeatureProtectedCrudAppService<
    TEntity,
    TEntityDto,
    TKey,
    TGetListInput,
    TCreateDto,
    TUpdateDto>
    : CrudAppService<TEntity, TEntityDto, TKey, TGetListInput, TCreateDto, TUpdateDto>
    where TEntity : class, IEntity<TKey>
    where TGetListInput : IPagedAndSortedResultRequest
{
    protected readonly ICurrentTenant CurrentTenant;
    protected readonly IFeatureChecker FeatureChecker;

    protected abstract string FeatureName { get; }
    protected abstract string TenantDefaultPermission { get; }
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

    protected virtual string MapPermissionForSide(string tenantPermission)
    {
        if (string.IsNullOrWhiteSpace(tenantPermission))
            throw new AbpAuthorizationException("Missing policy name.");

        if (CurrentTenant.IsAvailable)
            return tenantPermission;

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
        if (!CurrentTenant.IsAvailable) return;
        if (string.IsNullOrWhiteSpace(FeatureName)) return;

        if (!await FeatureChecker.IsEnabledAsync(FeatureName))
            throw new AbpAuthorizationException($"Feature '{FeatureName}' is disabled for this tenant.");
    }

    private async Task CheckPolicyRequiredAsync(string? policyName)
    {
        if (string.IsNullOrWhiteSpace(policyName))
            throw new AbpAuthorizationException("Missing policy name.");

        await AuthorizationService.CheckAsync(policyName);
    }

    protected override async Task CheckGetPolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(GetPolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckGetListPolicyAsync()
    {
        var policy = GetListPolicyName ?? GetPolicyName;
        await CheckPolicyRequiredAsync(MapPermissionForSide(policy));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckCreatePolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(CreatePolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckUpdatePolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(UpdatePolicyName));
        await EnsureFeatureAsync();
    }

    protected override async Task CheckDeletePolicyAsync()
    {
        await CheckPolicyRequiredAsync(MapPermissionForSide(DeletePolicyName));
        await EnsureFeatureAsync();
    }
}
