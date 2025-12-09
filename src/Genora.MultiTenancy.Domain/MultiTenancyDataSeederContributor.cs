using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Data;
using Volo.Abp.DependencyInjection;
using Volo.Abp.Identity;
using Volo.Abp.MultiTenancy;
using Volo.Abp.PermissionManagement;

namespace Genora.MultiTenancy;

public class MultiTenancyDataSeederContributor
    : IDataSeedContributor, ITransientDependency
{
    private readonly IIdentityDataSeeder _identityDataSeeder;
    private readonly IPermissionDefinitionManager _permissionDefinitionManager;
    private readonly IPermissionDataSeeder _permissionDataSeeder;

    public MultiTenancyDataSeederContributor(IIdentityDataSeeder identityDataSeeder, IPermissionDefinitionManager permissionDefinitionManager, IPermissionDataSeeder permissionDataSeeder)
    {
        _identityDataSeeder = identityDataSeeder;
        _permissionDefinitionManager = permissionDefinitionManager;
        _permissionDataSeeder = permissionDataSeeder;
    }

    public async Task SeedAsync(DataSeedContext context)
    {
        // ⭐ Chỉ chạy cho HOST (TenantId == null)
        if (context.TenantId != null) return;

        // Lấy email & pass từ DataSeedContext (nếu có), không thì mặc định
        var adminEmail = context.Properties.GetOrDefault("AdminEmail") as string ?? "xinchao@genora.vn";
        var adminPassword = context.Properties.GetOrDefault("AdminPassword") as string ?? "1q2w3E*";

        // 1) Tạo admin role + user (ABP tạo role 'admin' & user 'admin')
        await _identityDataSeeder.SeedAsync(adminEmail, adminPassword, tenantId: null);

        // 2) Gán toàn bộ permissions cho role 'admin' bên HOST
        var allHostPermissions = (await _permissionDefinitionManager.GetPermissionsAsync())
            .Where(p => p.MultiTenancySide.HasFlag(MultiTenancySides.Host))
            .Select(p => p.Name)
            .Distinct()
            .ToArray();

        await _permissionDataSeeder.SeedAsync(
            tenantId: null,
            providerName: RolePermissionValueProvider.ProviderName,
            providerKey: "admin",
            grantedPermissions: allHostPermissions
        );
    }
}