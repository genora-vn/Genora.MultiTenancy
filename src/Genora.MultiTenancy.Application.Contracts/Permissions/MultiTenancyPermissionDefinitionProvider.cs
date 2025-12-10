using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppGolfCourses;
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
        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppSettings

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

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppCustomerTypes

        var appCustomerTypeGroup = context.AddGroup(
            "MiniAppCustomerType",
            L("PermissionGroup:MiniAppCustomerType")
        );

        // ========== TENANT (bị ràng bởi Feature) ==========
        var appCustomerTypeTenantRoot = appCustomerTypeGroup.AddPermission(
            MultiTenancyPermissions.AppCustomerTypes.Default,
            L("Permission:MiniAppCustomerType"));

        appCustomerTypeTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        appCustomerTypeTenantRoot.RequireFeatures(AppCustomerTypeFeatures.Management);

        var appCustomerTypeTenantCreate = appCustomerTypeTenantRoot.AddChild(
            MultiTenancyPermissions.AppCustomerTypes.Create,
            L("Permission:MiniAppCustomerType.Create"));

        appCustomerTypeTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        appCustomerTypeTenantCreate.RequireFeatures(AppCustomerTypeFeatures.Management);

        var appCustomerTypeTenantEdit = appCustomerTypeTenantRoot.AddChild(
            MultiTenancyPermissions.AppCustomerTypes.Edit,
            L("Permission:MiniAppCustomerType.Edit"));

        appCustomerTypeTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        appCustomerTypeTenantEdit.RequireFeatures(AppCustomerTypeFeatures.Management);

        var appCustomerTypeTenantDelete = appCustomerTypeTenantRoot.AddChild(
            MultiTenancyPermissions.AppCustomerTypes.Delete,
            L("Permission:MiniAppCustomerType.Delete"));

        appCustomerTypeTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        appCustomerTypeTenantDelete.RequireFeatures(AppCustomerTypeFeatures.Management);

        // ========== HOST (không ràng Feature) ==========
        var appCustomerTypeGroupHost = context.AddGroup(
            "MiniAppCustomerTypeHost",
            L("PermissionGroup:MiniAppCustomerTypeHost")
        );

        var appCustomerTypeHostRoot = appCustomerTypeGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppCustomerTypes.Default,
            L("Permission:MiniAppCustomerType"));

        appCustomerTypeHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var appCustomerTypeHostCreate = appCustomerTypeHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCustomerTypes.Create,
            L("Permission:MiniAppCustomerType.Create"));

        appCustomerTypeHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var appCustomerTypeHostEdit = appCustomerTypeHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCustomerTypes.Edit,
            L("Permission:MiniAppCustomerType.Edit"));

        appCustomerTypeHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var appCustomerTypeHostDelete = appCustomerTypeHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCustomerTypes.Delete,
            L("Permission:MiniAppCustomerType.Delete"));

        appCustomerTypeHostDelete.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppGolfCourses

        var golfCourseGroup = context.AddGroup(
            "MiniAppGolfCourse",
            L("PermissionGroup:MiniAppGolfCourse"));

        // TENANT (bị ràng Feature)
        var golfCourseTenantRoot = golfCourseGroup.AddPermission(
            MultiTenancyPermissions.AppGolfCourses.Default,
            L("Permission:MiniAppGolfCourse"));

        golfCourseTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        golfCourseTenantRoot.RequireFeatures(AppGolfCourseFeatures.Management);

        var golfCourseTenantCreate = golfCourseTenantRoot.AddChild(
            MultiTenancyPermissions.AppGolfCourses.Create,
            L("Permission:MiniAppGolfCourse.Create"));

        golfCourseTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        golfCourseTenantCreate.RequireFeatures(AppGolfCourseFeatures.Management);

        var golfCourseTenantEdit = golfCourseTenantRoot.AddChild(
            MultiTenancyPermissions.AppGolfCourses.Edit,
            L("Permission:MiniAppGolfCourse.Edit"));

        golfCourseTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        golfCourseTenantEdit.RequireFeatures(AppGolfCourseFeatures.Management);

        var golfCourseTenantDelete = golfCourseTenantRoot.AddChild(
            MultiTenancyPermissions.AppGolfCourses.Delete,
            L("Permission:MiniAppGolfCourse.Delete"));

        golfCourseTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        golfCourseTenantDelete.RequireFeatures(AppGolfCourseFeatures.Management);

        // HOST (không ràng Feature)
        var golfCourseGroupHost = context.AddGroup(
            "MiniAppGolfCourseHost",
            L("PermissionGroup:MiniAppGolfCourseHost"));

        var golfCourseHostRoot = golfCourseGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppGolfCourses.Default,
            L("Permission:MiniAppGolfCourse"));

        golfCourseHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var golfCourseHostCreate = golfCourseHostRoot.AddChild(
            MultiTenancyPermissions.HostAppGolfCourses.Create,
            L("Permission:MiniAppGolfCourse.Create"));

        golfCourseHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var golfCourseHostEdit = golfCourseHostRoot.AddChild(
            MultiTenancyPermissions.HostAppGolfCourses.Edit,
            L("Permission:MiniAppGolfCourse.Edit"));

        golfCourseHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var golfCourseHostDelete = golfCourseHostRoot.AddChild(
            MultiTenancyPermissions.HostAppGolfCourses.Delete,
            L("Permission:MiniAppGolfCourse.Delete"));

        golfCourseHostDelete.MultiTenancySide = MultiTenancySides.Host;

        #endregion
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
