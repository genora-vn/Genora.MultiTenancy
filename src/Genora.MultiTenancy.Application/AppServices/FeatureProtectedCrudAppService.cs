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
    /// Mặc định:
    /// - Tenant side: dùng y nguyên permission
    /// - Host side: map (TenantDefault + suffix) -> (HostDefault + suffix)
    /// </summary>
    protected virtual string MapPermissionForSide(string tenantPermission)
    {
        // Strict: policy phải có
        if (string.IsNullOrWhiteSpace(tenantPermission))
            throw new AbpAuthorizationException("Missing policy name.");

        // Tenant side: dùng y nguyên
        if (CurrentTenant.IsAvailable)
            return tenantPermission;

        // Host side: map theo prefix nếu có đủ root
        if (string.IsNullOrWhiteSpace(TenantDefaultPermission) ||
            string.IsNullOrWhiteSpace(HostDefaultPermission))
        {
            // Không có root để map thì trả lại permission gốc (nhưng thường không nên xảy ra)
            return tenantPermission;
        }

        // Map theo prefix
        if (tenantPermission.StartsWith(TenantDefaultPermission))
        {
            var suffix = tenantPermission.Substring(TenantDefaultPermission.Length);
            return HostDefaultPermission + suffix;
        }

        // Không match prefix => fallback host root
        return HostDefaultPermission;
    }

    protected virtual async Task EnsureFeatureAsync()
    {
        // Chỉ Tenant mới bị ràng Feature
        if (!CurrentTenant.IsAvailable) return;

        if (string.IsNullOrWhiteSpace(FeatureName)) return;

        if (!await FeatureChecker.IsEnabledAsync(FeatureName))
            throw new AbpAuthorizationException($"Feature '{FeatureName}' is disabled for this tenant.");
    }

    private async Task CheckPolicyRequiredAsync(string? policyName)
    {
        // Strict: policy phải có, nếu null/empty => fail rõ
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
