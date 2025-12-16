using Genora.MultiTenancy.Features;
using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Features.AppCalendarSlots;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppGolfCourses;
using Genora.MultiTenancy.Features.AppMembershipTiers;
using Genora.MultiTenancy.Features.AppNewsFeatures;
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

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppMembershipTiers

        var membershipTierGroup = context.AddGroup(
            "MiniAppMembershipTier",
            L("PermissionGroup:MiniAppMembershipTier"));

        // TENANT (bị ràng Feature)
        var membershipTenantRoot = membershipTierGroup.AddPermission(
            MultiTenancyPermissions.AppMembershipTiers.Default,
            L("Permission:MiniAppMembershipTier"));

        membershipTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        membershipTenantRoot.RequireFeatures(AppMembershipTierFeatures.Management);

        var membershipTenantCreate = membershipTenantRoot.AddChild(
            MultiTenancyPermissions.AppMembershipTiers.Create,
            L("Permission:MiniAppMembershipTier.Create"));

        membershipTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        membershipTenantCreate.RequireFeatures(AppMembershipTierFeatures.Management);

        var membershipTenantEdit = membershipTenantRoot.AddChild(
            MultiTenancyPermissions.AppMembershipTiers.Edit,
            L("Permission:MiniAppMembershipTier.Edit"));

        membershipTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        membershipTenantEdit.RequireFeatures(AppMembershipTierFeatures.Management);

        var membershipTenantDelete = membershipTenantRoot.AddChild(
            MultiTenancyPermissions.AppMembershipTiers.Delete,
            L("Permission:MiniAppMembershipTier.Delete"));

        membershipTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        membershipTenantDelete.RequireFeatures(AppMembershipTierFeatures.Management);

        // HOST (không ràng Feature)
        var membershipTierGroupHost = context.AddGroup(
            "MiniAppMembershipTierHost",
            L("PermissionGroup:MiniAppMembershipTierHost"));

        var membershipHostRoot = membershipTierGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppMembershipTiers.Default,
            L("Permission:MiniAppMembershipTier"));

        membershipHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var membershipHostCreate = membershipHostRoot.AddChild(
            MultiTenancyPermissions.HostAppMembershipTiers.Create,
            L("Permission:MiniAppMembershipTier.Create"));

        membershipHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var membershipHostEdit = membershipHostRoot.AddChild(
            MultiTenancyPermissions.HostAppMembershipTiers.Edit,
            L("Permission:MiniAppMembershipTier.Edit"));

        membershipHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var membershipHostDelete = membershipHostRoot.AddChild(
            MultiTenancyPermissions.HostAppMembershipTiers.Delete,
            L("Permission:MiniAppMembershipTier.Delete"));

        membershipHostDelete.MultiTenancySide = MultiTenancySides.Host;
        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppCustomers

        var customerGroup = context.AddGroup(
            "MiniAppCustomer",
            L("PermissionGroup:MiniAppCustomer"));

        // TENANT (bị ràng Feature)
        var customerTenantRoot = customerGroup.AddPermission(
            MultiTenancyPermissions.AppCustomers.Default,
            L("Permission:MiniAppCustomer"));

        customerTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        customerTenantRoot.RequireFeatures(AppCustomerFeatures.Management);

        var customerTenantCreate = customerTenantRoot.AddChild(
            MultiTenancyPermissions.AppCustomers.Create,
            L("Permission:MiniAppCustomer.Create"));

        customerTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        customerTenantCreate.RequireFeatures(AppCustomerFeatures.Management);

        var customerTenantEdit = customerTenantRoot.AddChild(
            MultiTenancyPermissions.AppCustomers.Edit,
            L("Permission:MiniAppCustomer.Edit"));

        customerTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        customerTenantEdit.RequireFeatures(AppCustomerFeatures.Management);

        var customerTenantDelete = customerTenantRoot.AddChild(
            MultiTenancyPermissions.AppCustomers.Delete,
            L("Permission:MiniAppCustomer.Delete"));

        customerTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        customerTenantDelete.RequireFeatures(AppCustomerFeatures.Management);

        // HOST (không ràng Feature)
        var customerGroupHost = context.AddGroup(
            "MiniAppCustomerHost",
            L("PermissionGroup:MiniAppCustomerHost"));

        var customerHostRoot = customerGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppCustomers.Default,
            L("Permission:MiniAppCustomer"));

        customerHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var customerHostCreate = customerHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCustomers.Create,
            L("Permission:MiniAppCustomer.Create"));

        customerHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var customerHostEdit = customerHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCustomers.Edit,
            L("Permission:MiniAppCustomer.Edit"));

        customerHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var customerHostDelete = customerHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCustomers.Delete,
            L("Permission:MiniAppCustomer.Delete"));

        customerHostDelete.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppCalendarSlots

        var calendarGroup = context.AddGroup("MiniAppCalendarSlot", L("PermissionGroup:MiniAppCalendarSlot"));

        // TENANT (bị ràng Feature)
        var calendarTenantRoot = calendarGroup.AddPermission(
            MultiTenancyPermissions.AppCalendarSlots.Default,
            L("Permission:MiniAppCalendarSlot")
        );
        calendarTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        calendarTenantRoot.RequireFeatures(AppCalendarSlotFeatures.Management);

        var calendarTenantCreate = calendarTenantRoot.AddChild(
            MultiTenancyPermissions.AppCalendarSlots.Create,
            L("Permission:MiniAppCalendarSlot.Create")
        );
        calendarTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        calendarTenantCreate.RequireFeatures(AppCalendarSlotFeatures.Management);

        var calendarTenantEdit = calendarTenantRoot.AddChild(
            MultiTenancyPermissions.AppCalendarSlots.Edit,
            L("Permission:MiniAppCalendarSlot.Edit")
        );
        calendarTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        calendarTenantEdit.RequireFeatures(AppCalendarSlotFeatures.Management);

        var calendarTenantDelete = calendarTenantRoot.AddChild(
            MultiTenancyPermissions.AppCalendarSlots.Delete,
            L("Permission:MiniAppCalendarSlot.Delete")
        );
        calendarTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        calendarTenantDelete.RequireFeatures(AppCalendarSlotFeatures.Management);

        // HOST (không ràng Feature)
        var calendarHostGroup = context.AddGroup("MiniAppCalendarSlotHost", L("PermissionGroup:MiniAppCalendarSlotHost"));

        var calendarHostRoot = calendarHostGroup.AddPermission(
            MultiTenancyPermissions.HostAppCalendarSlots.Default,
            L("Permission:MiniAppCalendarSlot")
        );
        calendarHostRoot.MultiTenancySide = MultiTenancySides.Host;

        calendarHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCalendarSlots.Create,
            L("Permission:MiniAppCalendarSlot.Create")
        ).MultiTenancySide = MultiTenancySides.Host;

        calendarHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCalendarSlots.Edit,
            L("Permission:MiniAppCalendarSlot.Edit")
        ).MultiTenancySide = MultiTenancySides.Host;

        calendarHostRoot.AddChild(
            MultiTenancyPermissions.HostAppCalendarSlots.Delete,
            L("Permission:MiniAppCalendarSlot.Delete")
        ).MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị News

        // TENANT (bị ràng Feature)
        var newsGroup = context.AddGroup(
            "MiniAppNews",
            L("PermissionGroup:MiniAppNews")
        );

        var newsTenantRoot = newsGroup.AddPermission(
            MultiTenancyPermissions.AppNews.Default,
            L("Permission:MiniAppNews")
        );
        newsTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        newsTenantRoot.RequireFeatures(AppNewsFeatures.Management);

        var newsTenantCreate = newsTenantRoot.AddChild(
            MultiTenancyPermissions.AppNews.Create,
            L("Permission:MiniAppNews.Create")
        );
        newsTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        newsTenantCreate.RequireFeatures(AppNewsFeatures.Management);

        var newsTenantEdit = newsTenantRoot.AddChild(
            MultiTenancyPermissions.AppNews.Edit,
            L("Permission:MiniAppNews.Edit")
        );
        newsTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        newsTenantEdit.RequireFeatures(AppNewsFeatures.Management);

        var newsTenantDelete = newsTenantRoot.AddChild(
            MultiTenancyPermissions.AppNews.Delete,
            L("Permission:MiniAppNews.Delete")
        );
        newsTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        newsTenantDelete.RequireFeatures(AppNewsFeatures.Management);

        // HOST (không ràng Feature)
        var newsHostGroup = context.AddGroup(
            "MiniAppNewsHost",
            L("PermissionGroup:MiniAppNewsHost")
        );

        var newsHostRoot = newsHostGroup.AddPermission(
            MultiTenancyPermissions.HostAppNews.Default,
            L("Permission:MiniAppNews")
        );
        newsHostRoot.MultiTenancySide = MultiTenancySides.Host;

        newsHostRoot.AddChild(
            MultiTenancyPermissions.HostAppNews.Create,
            L("Permission:MiniAppNews.Create")
        ).MultiTenancySide = MultiTenancySides.Host;

        newsHostRoot.AddChild(
            MultiTenancyPermissions.HostAppNews.Edit,
            L("Permission:MiniAppNews.Edit")
        ).MultiTenancySide = MultiTenancySides.Host;

        newsHostRoot.AddChild(
            MultiTenancyPermissions.HostAppNews.Delete,
            L("Permission:MiniAppNews.Delete")
        ).MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị Bookings

        // TENANT (bị ràng Feature)
        var bookingGroup = context.AddGroup(
            "MiniAppBooking",
            L("PermissionGroup:MiniAppBooking")
        );

        var bookingTenantRoot = bookingGroup.AddPermission(
            MultiTenancyPermissions.AppBookings.Default,
            L("Permission:MiniAppBooking")
        );
        bookingTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        bookingTenantRoot.RequireFeatures(AppBookingFeatures.Management);

        var bookingTenantCreate = bookingTenantRoot.AddChild(
            MultiTenancyPermissions.AppBookings.Create,
            L("Permission:MiniAppBooking.Create")
        );
        bookingTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        bookingTenantCreate.RequireFeatures(AppBookingFeatures.Management);

        var bookingTenantEdit = bookingTenantRoot.AddChild(
            MultiTenancyPermissions.AppBookings.Edit,
            L("Permission:MiniAppBooking.Edit")
        );
        bookingTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        bookingTenantEdit.RequireFeatures(AppBookingFeatures.Management);

        var bookingTenantDelete = bookingTenantRoot.AddChild(
            MultiTenancyPermissions.AppBookings.Delete,
            L("Permission:MiniAppBooking.Delete")
        );
        bookingTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        bookingTenantDelete.RequireFeatures(AppBookingFeatures.Management);

        // HOST (không ràng Feature)
        var bookingGroupHost = context.AddGroup(
            "MiniAppBookingHost",
            L("PermissionGroup:MiniAppBookingHost"));

        var bookingHostRoot = bookingGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppBookings.Default,
            L("Permission:MiniAppBooking"));

        bookingHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var bookingHostCreate = bookingHostRoot.AddChild(
            MultiTenancyPermissions.HostAppBookings.Create,
            L("Permission:MiniAppBooking.Create"));

        bookingHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var bookingHostEdit = bookingHostRoot.AddChild(
            MultiTenancyPermissions.HostAppBookings.Edit,
            L("Permission:MiniAppBooking.Edit"));

        bookingHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var bookingHostDelete = bookingHostRoot.AddChild(
            MultiTenancyPermissions.HostAppBookings.Delete,
            L("Permission:MiniAppBooking.Delete"));

        bookingHostDelete.MultiTenancySide = MultiTenancySides.Host;

        #endregion
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
