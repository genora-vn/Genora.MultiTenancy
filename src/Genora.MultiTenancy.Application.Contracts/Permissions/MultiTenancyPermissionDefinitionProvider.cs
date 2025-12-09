using Genora.MultiTenancy.Features;
using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Permissions;

public class MultiTenancyPermissionDefinitionProvider : PermissionDefinitionProvider
{
    public override void Define(IPermissionDefinitionContext context)
    {
        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị BookStore

        var group = context.AddGroup("BookStore", L("PermissionGroup:BookStore"));

        // ========== TENANT (bị ràng bởi Feature) ==========
        var tRoot = group.AddPermission(MultiTenancyPermissions.Books.Default, L("Permission:Books"));
        tRoot.MultiTenancySide = MultiTenancySides.Tenant;
        tRoot.RequireFeatures(BookStoreFeatures.Management);

        var tCreate = tRoot.AddChild(MultiTenancyPermissions.Books.Create, L("Permission:Books.Create"));
        tCreate.MultiTenancySide = MultiTenancySides.Tenant;
        tCreate.RequireFeatures(BookStoreFeatures.Management);

        var tEdit = tRoot.AddChild(MultiTenancyPermissions.Books.Edit, L("Permission:Books.Edit"));
        tEdit.MultiTenancySide = MultiTenancySides.Tenant;
        tEdit.RequireFeatures(BookStoreFeatures.Management);

        var tDelete = tRoot.AddChild(MultiTenancyPermissions.Books.Delete, L("Permission:Books.Delete"));
        tDelete.MultiTenancySide = MultiTenancySides.Tenant;
        tDelete.RequireFeatures(BookStoreFeatures.Management);

        var groupHost = context.AddGroup("BookStoreDev", L("PermissionGroup:BookStoreDev"));
        // ========== HOST (không ràng Feature) ==========
        var hRoot = groupHost.AddPermission(MultiTenancyPermissions.HostBooks.Default, L("Permission:Books"));
        hRoot.MultiTenancySide = MultiTenancySides.Host;

        var hCreate = hRoot.AddChild(MultiTenancyPermissions.HostBooks.Create, L("Permission:Books.Create"));
        hCreate.MultiTenancySide = MultiTenancySides.Host;

        var hEdit = hRoot.AddChild(MultiTenancyPermissions.HostBooks.Edit, L("Permission:Books.Edit"));
        hEdit.MultiTenancySide = MultiTenancySides.Host;

        var hDelete = hRoot.AddChild(MultiTenancyPermissions.HostBooks.Delete, L("Permission:Books.Delete"));
        hDelete.MultiTenancySide = MultiTenancySides.Host;
        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị BookStore

        var appSettingGroup = context.AddGroup("MiniAppSetting", L("PermissionGroup:MiniAppSetting"));

        // ========== TENANT (bị ràng bởi Feature) ==========
        var appSettingTenantRoot = appSettingGroup.AddPermission(MultiTenancyPermissions.AppSettings.Default, L("Permission:MiniAppSetting"));
        appSettingTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        appSettingTenantRoot.RequireFeatures(AppSettingFeatures.Management);

        var appSettingTenantCreate = appSettingTenantRoot.AddChild(MultiTenancyPermissions.AppSettings.Create, L("Permission:MiniAppSetting.Create"));
        appSettingTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        appSettingTenantCreate.RequireFeatures(AppSettingFeatures.Management);

        var appSettingTenantEdit = appSettingTenantRoot.AddChild(MultiTenancyPermissions.AppSettings.Edit, L("Permission:MiniAppSetting.Edit"));
        appSettingTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        appSettingTenantEdit.RequireFeatures(AppSettingFeatures.Management);

        var appSettingTenantDelete = appSettingTenantRoot.AddChild(MultiTenancyPermissions.AppSettings.Delete, L("Permission:MiniAppSetting.Delete"));
        appSettingTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        appSettingTenantDelete.RequireFeatures(AppSettingFeatures.Management);

        var appSettingGroupHost = context.AddGroup("MiniAppSettingHost", L("PermissionGroup:MiniAppSettingHost"));
        // ========== HOST (không ràng Feature) ==========
        var appSettingHostRoot = appSettingGroupHost.AddPermission(MultiTenancyPermissions.HostAppSettings.Default, L("Permission:MiniAppSetting"));
        appSettingHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var appSettingHostCreate = appSettingHostRoot.AddChild(MultiTenancyPermissions.HostAppSettings.Create, L("Permission:MiniAppSetting.Create"));
        appSettingHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var appSettingHostEdit = appSettingHostRoot.AddChild(MultiTenancyPermissions.HostAppSettings.Edit, L("Permission:MiniAppSetting.Edit"));
        appSettingHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var appSettingHostDelete = appSettingHostRoot.AddChild(MultiTenancyPermissions.HostAppSettings.Delete, L("Permission:MiniAppSetting.Delete"));
        appSettingHostDelete.MultiTenancySide = MultiTenancySides.Host;
        #endregion
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
