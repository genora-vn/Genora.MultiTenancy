using Genora.MultiTenancy.Features;
using Genora.MultiTenancy.Features.AppBookingFeatures;
using Genora.MultiTenancy.Features.AppCalendarSlots;
using Genora.MultiTenancy.Features.AppCustomers;
using Genora.MultiTenancy.Features.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppEmails;
using Genora.MultiTenancy.Features.AppFnbFeatures;
using Genora.MultiTenancy.Features.AppProshopFeatures;
using Genora.MultiTenancy.Features.AppGolfCourses;
using Genora.MultiTenancy.Features.AppMembershipTiers;
using Genora.MultiTenancy.Features.AppNewsFeatures;
using Genora.MultiTenancy.Features.AppPromotionTypes;
using Genora.MultiTenancy.Features.AppPromotionPolicies;
using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Features.AppSpecialDates;
using Genora.MultiTenancy.Features.AppPaymentConfigurationFeatures;
using Genora.MultiTenancy.Features.AppZaloAuths;
using Genora.MultiTenancy.Features.AppZaloLogs;
using Genora.MultiTenancy.Localization;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Features;
using Volo.Abp.Localization;
using Volo.Abp.MultiTenancy;
using static Genora.MultiTenancy.Permissions.MultiTenancyPermissions;

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

        // AppHomePageConfig (TENANT - bị ràng Feature)
        var homePageConfig = appSettingGroup.AddPermission(
            MultiTenancyPermissions.AppHomePageConfigs.Default,
            L("Permission:AppHomePageConfig")
        );
        homePageConfig.MultiTenancySide = MultiTenancySides.Tenant;
        homePageConfig.RequireFeatures(Genora.MultiTenancy.Features.AppHomePages.AppHomePageFeatures.Management);

        var homePageConfigEdit = homePageConfig.AddChild(
            MultiTenancyPermissions.AppHomePageConfigs.Edit,
            L("Permission:AppHomePageConfig.Edit")
        );
        homePageConfigEdit.MultiTenancySide = MultiTenancySides.Tenant;
        homePageConfigEdit.RequireFeatures(Genora.MultiTenancy.Features.AppHomePages.AppHomePageFeatures.Management);

        // Cấu hình thanh toán (TENANT - ràng Feature)
        var paymentTenantRoot = appSettingGroup.AddPermission(MultiTenancyPermissions.AppPaymentConfigurations.Default, L("Permission:AppPaymentConfigurations"));
        paymentTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        paymentTenantRoot.RequireFeatures(AppPaymentConfigurationFeatures.Management);
        var paymentTenantCreate = paymentTenantRoot.AddChild(MultiTenancyPermissions.AppPaymentConfigurations.Create, L("Permission:AppPaymentConfigurations.Create"));
        paymentTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        paymentTenantCreate.RequireFeatures(AppPaymentConfigurationFeatures.Management);
        var paymentTenantEdit = paymentTenantRoot.AddChild(MultiTenancyPermissions.AppPaymentConfigurations.Edit, L("Permission:AppPaymentConfigurations.Edit"));
        paymentTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        paymentTenantEdit.RequireFeatures(AppPaymentConfigurationFeatures.Management);
        var paymentTenantDelete = paymentTenantRoot.AddChild(MultiTenancyPermissions.AppPaymentConfigurations.Delete, L("Permission:AppPaymentConfigurations.Delete"));
        paymentTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        paymentTenantDelete.RequireFeatures(AppPaymentConfigurationFeatures.Management);

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

        // HostAppHomePageConfig (HOST - không ràng Feature)
        var homePageConfigHost = appSettingGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppHomePageConfigs.Default,
            L("Permission:HostAppHomePageConfig")
        );
        homePageConfigHost.MultiTenancySide = MultiTenancySides.Host;

        var homePageConfigHostEdit = homePageConfigHost.AddChild(
            MultiTenancyPermissions.HostAppHomePageConfigs.Edit,
            L("Permission:HostAppHomePageConfig.Edit")
        );
        homePageConfigHostEdit.MultiTenancySide = MultiTenancySides.Host;

        // Cấu hình thanh toán (HOST - không ràng Feature)
        var paymentHostRoot = appSettingGroupHost.AddPermission(MultiTenancyPermissions.HostAppPaymentConfigurations.Default, L("Permission:AppPaymentConfigurations"));
        paymentHostRoot.MultiTenancySide = MultiTenancySides.Host;
        paymentHostRoot.AddChild(MultiTenancyPermissions.HostAppPaymentConfigurations.Create, L("Permission:AppPaymentConfigurations.Create")).MultiTenancySide = MultiTenancySides.Host;
        paymentHostRoot.AddChild(MultiTenancyPermissions.HostAppPaymentConfigurations.Edit,   L("Permission:AppPaymentConfigurations.Edit"))  .MultiTenancySide = MultiTenancySides.Host;
        paymentHostRoot.AddChild(MultiTenancyPermissions.HostAppPaymentConfigurations.Delete, L("Permission:AppPaymentConfigurations.Delete")).MultiTenancySide = MultiTenancySides.Host;

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

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị ZaloAuth + ZaloLogs

        // =====================
        // TENANT (bị ràng Feature)
        // =====================
        var zaloAuthGroup = context.AddGroup(
            "MiniAppZaloAuth",
            L("PermissionGroup:MiniAppZaloAuth")
        );

        var zaloAuthTenantRoot = zaloAuthGroup.AddPermission(
            MultiTenancyPermissions.AppZaloAuths.Default,
            L("Permission:MiniAppZaloAuth")
        );
        zaloAuthTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        zaloAuthTenantRoot.RequireFeatures(AppZaloAuthFeatures.Management);

        var zaloAuthTenantCreate = zaloAuthTenantRoot.AddChild(
            MultiTenancyPermissions.AppZaloAuths.Create,
            L("Permission:MiniAppZaloAuth.Create")
        );
        zaloAuthTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        zaloAuthTenantCreate.RequireFeatures(AppZaloAuthFeatures.Management);

        var zaloAuthTenantEdit = zaloAuthTenantRoot.AddChild(
            MultiTenancyPermissions.AppZaloAuths.Edit,
            L("Permission:MiniAppZaloAuth.Edit")
        );
        zaloAuthTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        zaloAuthTenantEdit.RequireFeatures(AppZaloAuthFeatures.Management);

        var zaloAuthTenantDelete = zaloAuthTenantRoot.AddChild(
            MultiTenancyPermissions.AppZaloAuths.Delete,
            L("Permission:MiniAppZaloAuth.Delete")
        );
        zaloAuthTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        zaloAuthTenantDelete.RequireFeatures(AppZaloAuthFeatures.Management);

        // ✅ TENANT: Zalo Logs (bị ràng Feature riêng)
        var zaloLogTenantView = zaloAuthTenantRoot.AddChild(
            MultiTenancyPermissions.AppZaloLogs.Default,
            L("Permission:AppZaloLogs")
        );
        zaloLogTenantView.MultiTenancySide = MultiTenancySides.Tenant;
        zaloLogTenantView.RequireFeatures(AppZaloLogFeatures.Management);


        // =====================
        // HOST (không ràng Feature)
        // =====================
        var zaloAuthGroupHost = context.AddGroup(
            "MiniAppZaloAuthHost",
            L("PermissionGroup:MiniAppZaloAuthHost")
        );

        var zaloAuthHostRoot = zaloAuthGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppZaloAuths.Default,
            L("Permission:MiniAppZaloAuth")
        );
        zaloAuthHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var zaloAuthHostCreate = zaloAuthHostRoot.AddChild(
            MultiTenancyPermissions.HostAppZaloAuths.Create,
            L("Permission:MiniAppZaloAuth.Create")
        );
        zaloAuthHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var zaloAuthHostEdit = zaloAuthHostRoot.AddChild(
            MultiTenancyPermissions.HostAppZaloAuths.Edit,
            L("Permission:MiniAppZaloAuth.Edit")
        );
        zaloAuthHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var zaloAuthHostDelete = zaloAuthHostRoot.AddChild(
            MultiTenancyPermissions.HostAppZaloAuths.Delete,
            L("Permission:MiniAppZaloAuth.Delete")
        );
        zaloAuthHostDelete.MultiTenancySide = MultiTenancySides.Host;

        // ✅ HOST: Zalo Logs (set MultiTenancySide=Host để tenant không bị thấy “menu rác”)
        var zaloLogHostView = zaloAuthHostRoot.AddChild(
            MultiTenancyPermissions.HostAppZaloLogs.Default,
            L("Permission:HostAppZaloLogs")
        );
        zaloLogHostView.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppPromotionType
        var appPromotionTypeGroup = context.AddGroup(
            "MiniAppPromotionType",
            L("PermissionGroup:MiniAppPromotionType")
        );

        // ========== TENANT (bị ràng bởi Feature) ==========
        var appPromotionTypeTenantRoot = appPromotionTypeGroup.AddPermission(
            MultiTenancyPermissions.AppPromotionType.Default,
            L("Permission:MiniAppPromotionType"));

        appPromotionTypeTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        appPromotionTypeTenantRoot.RequireFeatures(AppPromotionTypeFeature.Management);

        var appPromotionTypeTenantCreate = appPromotionTypeTenantRoot.AddChild(
            MultiTenancyPermissions.AppPromotionType.Create,
            L("Permission:MiniAppPromotionType.Create"));

        appPromotionTypeTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        appPromotionTypeTenantCreate.RequireFeatures(AppPromotionTypeFeature.Management);

        var appPromotionTypeTenantEdit = appPromotionTypeTenantRoot.AddChild(
            MultiTenancyPermissions.AppPromotionType.Edit,
            L("Permission:MiniAppPromotionType.Edit"));

        appCustomerTypeTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        appCustomerTypeTenantEdit.RequireFeatures(AppPromotionTypeFeature.Management);

        var appPromotionTypeTenantDelete = appPromotionTypeTenantRoot.AddChild(
            MultiTenancyPermissions.AppPromotionType.Delete,
            L("Permission:MiniAppPromotionType.Delete"));

        appCustomerTypeTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        appCustomerTypeTenantDelete.RequireFeatures(AppPromotionTypeFeature.Management);

        // ========== HOST (không ràng Feature) ==========
        var appPromotionTypeGroupHost = context.AddGroup(
            "MiniAppPromotionTypeHost",
            L("PermissionGroup:MiniAppPromotionTypeHost")
        );

        var appPromotionTypeHostRoot = appPromotionTypeGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppPromotionType.Default,
            L("Permission:MiniAppPromotionType"));

        appPromotionTypeHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var appPromotionTypeHostCreate = appPromotionTypeHostRoot.AddChild(
            MultiTenancyPermissions.HostAppPromotionType.Create,
            L("Permission:MiniAppPromotionType.Create"));

        appPromotionTypeHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var appPromotionTypeHostEdit = appPromotionTypeHostRoot.AddChild(
            MultiTenancyPermissions.HostAppPromotionType.Edit,
            L("Permission:MiniAppPromotionType.Edit"));

        appPromotionTypeHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var appPromotionTypeHostDelete = appPromotionTypeHostRoot.AddChild(
            MultiTenancyPermissions.HostAppPromotionType.Delete,
            L("Permission:MiniAppPromotionType.Delete"));

        appPromotionTypeHostDelete.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền cho tính năng quản trị AppPromotionPolicies (Chính sách hoãn hủy)
        // ========== TENANT (bị ràng bởi Feature) ==========
        var appPromotionPolicyGroup = context.AddGroup(
            "MiniAppPromotionPolicy",
            L("PermissionGroup:MiniAppPromotionPolicy")
        );

        var appPromotionPolicyTenantRoot = appPromotionPolicyGroup.AddPermission(
            MultiTenancyPermissions.AppPromotionPolicies.Default,
            L("Permission:MiniAppPromotionPolicy"));
        appPromotionPolicyTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        appPromotionPolicyTenantRoot.RequireFeatures(AppPromotionPolicyFeatures.Management);

        var appPromotionPolicyTenantCreate = appPromotionPolicyTenantRoot.AddChild(
            MultiTenancyPermissions.AppPromotionPolicies.Create,
            L("Permission:MiniAppPromotionPolicy.Create"));
        appPromotionPolicyTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        appPromotionPolicyTenantCreate.RequireFeatures(AppPromotionPolicyFeatures.Management);

        var appPromotionPolicyTenantEdit = appPromotionPolicyTenantRoot.AddChild(
            MultiTenancyPermissions.AppPromotionPolicies.Edit,
            L("Permission:MiniAppPromotionPolicy.Edit"));
        appPromotionPolicyTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        appPromotionPolicyTenantEdit.RequireFeatures(AppPromotionPolicyFeatures.Management);

        var appPromotionPolicyTenantDelete = appPromotionPolicyTenantRoot.AddChild(
            MultiTenancyPermissions.AppPromotionPolicies.Delete,
            L("Permission:MiniAppPromotionPolicy.Delete"));
        appPromotionPolicyTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        appPromotionPolicyTenantDelete.RequireFeatures(AppPromotionPolicyFeatures.Management);

        // ========== HOST (không ràng Feature) ==========
        var appPromotionPolicyGroupHost = context.AddGroup(
            "MiniAppPromotionPolicyHost",
            L("PermissionGroup:MiniAppPromotionPolicyHost")
        );

        var appPromotionPolicyHostRoot = appPromotionPolicyGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppPromotionPolicies.Default,
            L("Permission:MiniAppPromotionPolicy"));
        appPromotionPolicyHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var appPromotionPolicyHostCreate = appPromotionPolicyHostRoot.AddChild(
            MultiTenancyPermissions.HostAppPromotionPolicies.Create,
            L("Permission:MiniAppPromotionPolicy.Create"));
        appPromotionPolicyHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var appPromotionPolicyHostEdit = appPromotionPolicyHostRoot.AddChild(
            MultiTenancyPermissions.HostAppPromotionPolicies.Edit,
            L("Permission:MiniAppPromotionPolicy.Edit"));
        appPromotionPolicyHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var appPromotionPolicyHostDelete = appPromotionPolicyHostRoot.AddChild(
            MultiTenancyPermissions.HostAppPromotionPolicies.Delete,
            L("Permission:MiniAppPromotionPolicy.Delete"));
        appPromotionPolicyHostDelete.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppSpecialDates

        // TENANT (bị ràng Feature)
        var specialDateGroup = context.AddGroup(
            "MiniAppSpecialDate",
            L("PermissionGroup:MiniAppSpecialDate")
        );

        var specialDateTenantRoot = specialDateGroup.AddPermission(
            MultiTenancyPermissions.AppSpecialDates.Default,
            L("Permission:MiniAppSpecialDate")
        );
        specialDateTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        specialDateTenantRoot.RequireFeatures(AppSpecialDateFeatures.Management);

        var specialDateTenantCreate = specialDateTenantRoot.AddChild(
            MultiTenancyPermissions.AppSpecialDates.Create,
            L("Permission:MiniAppSpecialDate.Create")
        );
        specialDateTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        specialDateTenantCreate.RequireFeatures(AppSpecialDateFeatures.Management);

        var specialDateTenantEdit = specialDateTenantRoot.AddChild(
            MultiTenancyPermissions.AppSpecialDates.Edit,
            L("Permission:MiniAppSpecialDate.Edit")
        );
        specialDateTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        specialDateTenantEdit.RequireFeatures(AppSpecialDateFeatures.Management);

        var specialDateTenantDelete = specialDateTenantRoot.AddChild(
            MultiTenancyPermissions.AppSpecialDates.Delete,
            L("Permission:MiniAppSpecialDate.Delete")
        );
        specialDateTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        specialDateTenantDelete.RequireFeatures(AppSpecialDateFeatures.Management);

        // HOST (không ràng Feature)
        var specialDateGroupHost = context.AddGroup(
            "MiniAppSpecialDateHost",
            L("PermissionGroup:MiniAppSpecialDateHost"));

        var specialDateHostRoot = specialDateGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppSpecialDates.Default,
            L("Permission:MiniAppSpecialDate"));

        specialDateHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var specialDateHostCreate = specialDateHostRoot.AddChild(
            MultiTenancyPermissions.HostAppSpecialDates.Create,
            L("Permission:MiniAppSpecialDate.Create"));

        specialDateHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var specialDateHostEdit = specialDateHostRoot.AddChild(
            MultiTenancyPermissions.HostAppSpecialDates.Edit,
            L("Permission:MiniAppSpecialDate.Edit"));

        specialDateHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var specialDateHostDelete = specialDateHostRoot.AddChild(
            MultiTenancyPermissions.HostAppSpecialDates.Delete,
            L("Permission:MiniAppSpecialDate.Delete"));

        specialDateHostDelete.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Cấu hình quyền Thêm / Sửa / Xóa cho tính năng quản trị AppEmails

        // TENANT (bị ràng Feature)
        var emailGroup = context.AddGroup(
            "MiniAppEmail",
            L("PermissionGroup:MiniAppEmail")
        );

        var emailTenantRoot = emailGroup.AddPermission(
            MultiTenancyPermissions.AppEmails.Default,
            L("Permission:MiniAppEmail")
        );
        emailTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        emailTenantRoot.RequireFeatures(AppEmailFeatures.Management);

        var emailTenantCreate = emailTenantRoot.AddChild(
            MultiTenancyPermissions.AppEmails.Create,
            L("Permission:MiniAppEmail.Create")
        );
        emailTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        emailTenantCreate.RequireFeatures(AppEmailFeatures.Management);

        var emailTenantEdit = emailTenantRoot.AddChild(
            MultiTenancyPermissions.AppEmails.Edit,
            L("Permission:MiniAppEmail.Edit")
        );
        emailTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        emailTenantEdit.RequireFeatures(AppEmailFeatures.Management);

        var emailTenantDelete = emailTenantRoot.AddChild(
            MultiTenancyPermissions.AppEmails.Delete,
            L("Permission:MiniAppEmail.Delete")
        );
        emailTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        emailTenantDelete.RequireFeatures(AppEmailFeatures.Management);

        var emailTenantSend = emailTenantRoot.AddChild(
            MultiTenancyPermissions.AppEmails.Send,
            L("Permission:MiniAppEmail.Send")
        );
        emailTenantSend.MultiTenancySide = MultiTenancySides.Tenant;
        emailTenantSend.RequireFeatures(AppEmailFeatures.Management);

        var emailTenantResend = emailTenantRoot.AddChild(
            MultiTenancyPermissions.AppEmails.Resend,
            L("Permission:MiniAppEmail.Resend")
        );
        emailTenantResend.MultiTenancySide = MultiTenancySides.Tenant;
        emailTenantResend.RequireFeatures(AppEmailFeatures.Management);

        // HOST (không ràng Feature)
        var emailGroupHost = context.AddGroup(
            "MiniAppEmailHost",
            L("PermissionGroup:MiniAppEmailHost"));

        var emailHostRoot = emailGroupHost.AddPermission(
            MultiTenancyPermissions.HostAppEmails.Default,
            L("Permission:MiniAppEmail"));

        emailHostRoot.MultiTenancySide = MultiTenancySides.Host;

        var emailHostCreate = emailHostRoot.AddChild(
            MultiTenancyPermissions.HostAppEmails.Create,
            L("Permission:MiniAppEmail.Create"));

        emailHostCreate.MultiTenancySide = MultiTenancySides.Host;

        var emailHostEdit = emailHostRoot.AddChild(
            MultiTenancyPermissions.HostAppEmails.Edit,
            L("Permission:MiniAppEmail.Edit"));

        emailHostEdit.MultiTenancySide = MultiTenancySides.Host;

        var emailHostDelete = emailHostRoot.AddChild(
            MultiTenancyPermissions.HostAppEmails.Delete,
            L("Permission:MiniAppEmail.Delete"));

        emailHostDelete.MultiTenancySide = MultiTenancySides.Host;

        var emailHostSend = emailHostRoot.AddChild(
           MultiTenancyPermissions.HostAppEmails.Send,
           L("Permission:MiniAppEmail.Send"));

        emailHostSend.MultiTenancySide = MultiTenancySides.Host;

        var emailHostResend = emailHostRoot.AddChild(
           MultiTenancyPermissions.HostAppEmails.Resend,
           L("Permission:MiniAppEmail.Resend"));

        emailHostResend.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Quản lý FnB — 1 group gộp (Tenant + Host tách)

        // ── TENANT ──────────────────────────────────────────────────────────
        var fnbGroup = context.AddGroup("MiniAppFnb", L("PermissionGroup:MiniAppFnb"));

        // 1. Danh mục FnB
        var fnbCatRoot = fnbGroup.AddPermission(MultiTenancyPermissions.AppFnbCategories.Default, L("Permission:MiniAppFnbCategories"));
        fnbCatRoot.MultiTenancySide = MultiTenancySides.Tenant;
        fnbCatRoot.RequireFeatures(AppFnbFeatures.Management);
        var fnbCatCreate = fnbCatRoot.AddChild(MultiTenancyPermissions.AppFnbCategories.Create, L("Permission:MiniAppFnbCategories.Create"));
        fnbCatCreate.MultiTenancySide = MultiTenancySides.Tenant; fnbCatCreate.RequireFeatures(AppFnbFeatures.Management);
        var fnbCatEdit = fnbCatRoot.AddChild(MultiTenancyPermissions.AppFnbCategories.Edit, L("Permission:MiniAppFnbCategories.Edit"));
        fnbCatEdit.MultiTenancySide = MultiTenancySides.Tenant; fnbCatEdit.RequireFeatures(AppFnbFeatures.Management);
        var fnbCatDelete = fnbCatRoot.AddChild(MultiTenancyPermissions.AppFnbCategories.Delete, L("Permission:MiniAppFnbCategories.Delete"));
        fnbCatDelete.MultiTenancySide = MultiTenancySides.Tenant; fnbCatDelete.RequireFeatures(AppFnbFeatures.Management);

        // 2. Món ăn / Đồ uống
        var fnbItemRoot = fnbGroup.AddPermission(MultiTenancyPermissions.AppFnbItems.Default, L("Permission:MiniAppFnbItems"));
        fnbItemRoot.MultiTenancySide = MultiTenancySides.Tenant;
        fnbItemRoot.RequireFeatures(AppFnbFeatures.Management);
        var fnbItemCreate = fnbItemRoot.AddChild(MultiTenancyPermissions.AppFnbItems.Create, L("Permission:MiniAppFnbItems.Create"));
        fnbItemCreate.MultiTenancySide = MultiTenancySides.Tenant; fnbItemCreate.RequireFeatures(AppFnbFeatures.Management);
        var fnbItemEdit = fnbItemRoot.AddChild(MultiTenancyPermissions.AppFnbItems.Edit, L("Permission:MiniAppFnbItems.Edit"));
        fnbItemEdit.MultiTenancySide = MultiTenancySides.Tenant; fnbItemEdit.RequireFeatures(AppFnbFeatures.Management);
        var fnbItemDelete = fnbItemRoot.AddChild(MultiTenancyPermissions.AppFnbItems.Delete, L("Permission:MiniAppFnbItems.Delete"));
        fnbItemDelete.MultiTenancySide = MultiTenancySides.Tenant; fnbItemDelete.RequireFeatures(AppFnbFeatures.Management);

        // 3. Đơn hàng FnB
        var fnbOrderRoot = fnbGroup.AddPermission(MultiTenancyPermissions.AppFnbOrders.Default, L("Permission:MiniAppFnbOrders"));
        fnbOrderRoot.MultiTenancySide = MultiTenancySides.Tenant;
        fnbOrderRoot.RequireFeatures(AppFnbFeatures.Management);
        var fnbOrderCreate = fnbOrderRoot.AddChild(MultiTenancyPermissions.AppFnbOrders.Create, L("Permission:MiniAppFnbOrders.Create"));
        fnbOrderCreate.MultiTenancySide = MultiTenancySides.Tenant; fnbOrderCreate.RequireFeatures(AppFnbFeatures.Management);
        var fnbOrderEdit = fnbOrderRoot.AddChild(MultiTenancyPermissions.AppFnbOrders.Edit, L("Permission:MiniAppFnbOrders.Edit"));
        fnbOrderEdit.MultiTenancySide = MultiTenancySides.Tenant; fnbOrderEdit.RequireFeatures(AppFnbFeatures.Management);
        var fnbOrderDelete = fnbOrderRoot.AddChild(MultiTenancyPermissions.AppFnbOrders.Delete, L("Permission:MiniAppFnbOrders.Delete"));
        fnbOrderDelete.MultiTenancySide = MultiTenancySides.Tenant; fnbOrderDelete.RequireFeatures(AppFnbFeatures.Management);

        // 4. Kitchen Board
        var fnbKitchenRoot = fnbGroup.AddPermission(MultiTenancyPermissions.AppFnbKitchenBoard.Default, L("Permission:MiniAppFnbKitchenBoard"));
        fnbKitchenRoot.MultiTenancySide = MultiTenancySides.Tenant;
        fnbKitchenRoot.RequireFeatures(AppFnbFeatures.Management);

        // ── HOST ─────────────────────────────────────────────────────────────
        var fnbGroupHost = context.AddGroup("MiniAppFnbHost", L("PermissionGroup:MiniAppFnbHost"));

        var fnbCatHostRoot = fnbGroupHost.AddPermission(MultiTenancyPermissions.HostAppFnbCategories.Default, L("Permission:MiniAppFnbCategories"));
        fnbCatHostRoot.MultiTenancySide = MultiTenancySides.Host;
        fnbCatHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbCategories.Create, L("Permission:MiniAppFnbCategories.Create")).MultiTenancySide = MultiTenancySides.Host;
        fnbCatHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbCategories.Edit,   L("Permission:MiniAppFnbCategories.Edit"))  .MultiTenancySide = MultiTenancySides.Host;
        fnbCatHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbCategories.Delete, L("Permission:MiniAppFnbCategories.Delete")).MultiTenancySide = MultiTenancySides.Host;

        var fnbItemHostRoot = fnbGroupHost.AddPermission(MultiTenancyPermissions.HostAppFnbItems.Default, L("Permission:MiniAppFnbItems"));
        fnbItemHostRoot.MultiTenancySide = MultiTenancySides.Host;
        fnbItemHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbItems.Create, L("Permission:MiniAppFnbItems.Create")).MultiTenancySide = MultiTenancySides.Host;
        fnbItemHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbItems.Edit,   L("Permission:MiniAppFnbItems.Edit"))  .MultiTenancySide = MultiTenancySides.Host;
        fnbItemHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbItems.Delete, L("Permission:MiniAppFnbItems.Delete")).MultiTenancySide = MultiTenancySides.Host;

        var fnbOrderHostRoot = fnbGroupHost.AddPermission(MultiTenancyPermissions.HostAppFnbOrders.Default, L("Permission:MiniAppFnbOrders"));
        fnbOrderHostRoot.MultiTenancySide = MultiTenancySides.Host;
        fnbOrderHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbOrders.Create, L("Permission:MiniAppFnbOrders.Create")).MultiTenancySide = MultiTenancySides.Host;
        fnbOrderHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbOrders.Edit,   L("Permission:MiniAppFnbOrders.Edit"))  .MultiTenancySide = MultiTenancySides.Host;
        fnbOrderHostRoot.AddChild(MultiTenancyPermissions.HostAppFnbOrders.Delete, L("Permission:MiniAppFnbOrders.Delete")).MultiTenancySide = MultiTenancySides.Host;

        var fnbKitchenHostRoot = fnbGroupHost.AddPermission(MultiTenancyPermissions.HostAppFnbKitchenBoard.Default, L("Permission:MiniAppFnbKitchenBoard"));
        fnbKitchenHostRoot.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Quản lý Proshop — 1 group gộp (Tenant + Host tách)

        // ── TENANT ──────────────────────────────────────────────────────────
        var proGroup = context.AddGroup("MiniAppProshop", L("PermissionGroup:MiniAppProshop"));

        // 1. Danh mục Proshop
        var proCatRoot = proGroup.AddPermission(MultiTenancyPermissions.AppProCategories.Default, L("Permission:MiniAppProCategories"));
        proCatRoot.MultiTenancySide = MultiTenancySides.Tenant;
        proCatRoot.RequireFeatures(AppProshopFeatures.Management);
        var proCatCreate = proCatRoot.AddChild(MultiTenancyPermissions.AppProCategories.Create, L("Permission:MiniAppProCategories.Create"));
        proCatCreate.MultiTenancySide = MultiTenancySides.Tenant; proCatCreate.RequireFeatures(AppProshopFeatures.Management);
        var proCatEdit = proCatRoot.AddChild(MultiTenancyPermissions.AppProCategories.Edit, L("Permission:MiniAppProCategories.Edit"));
        proCatEdit.MultiTenancySide = MultiTenancySides.Tenant; proCatEdit.RequireFeatures(AppProshopFeatures.Management);
        var proCatDelete = proCatRoot.AddChild(MultiTenancyPermissions.AppProCategories.Delete, L("Permission:MiniAppProCategories.Delete"));
        proCatDelete.MultiTenancySide = MultiTenancySides.Tenant; proCatDelete.RequireFeatures(AppProshopFeatures.Management);

        // 2. Sản phẩm Proshop
        var proItemRoot = proGroup.AddPermission(MultiTenancyPermissions.AppProItems.Default, L("Permission:MiniAppProItems"));
        proItemRoot.MultiTenancySide = MultiTenancySides.Tenant;
        proItemRoot.RequireFeatures(AppProshopFeatures.Management);
        var proItemCreate = proItemRoot.AddChild(MultiTenancyPermissions.AppProItems.Create, L("Permission:MiniAppProItems.Create"));
        proItemCreate.MultiTenancySide = MultiTenancySides.Tenant; proItemCreate.RequireFeatures(AppProshopFeatures.Management);
        var proItemEdit = proItemRoot.AddChild(MultiTenancyPermissions.AppProItems.Edit, L("Permission:MiniAppProItems.Edit"));
        proItemEdit.MultiTenancySide = MultiTenancySides.Tenant; proItemEdit.RequireFeatures(AppProshopFeatures.Management);
        var proItemDelete = proItemRoot.AddChild(MultiTenancyPermissions.AppProItems.Delete, L("Permission:MiniAppProItems.Delete"));
        proItemDelete.MultiTenancySide = MultiTenancySides.Tenant; proItemDelete.RequireFeatures(AppProshopFeatures.Management);

        // 3. Đơn hàng Proshop
        var proOrderRoot = proGroup.AddPermission(MultiTenancyPermissions.AppProOrders.Default, L("Permission:MiniAppProOrders"));
        proOrderRoot.MultiTenancySide = MultiTenancySides.Tenant;
        proOrderRoot.RequireFeatures(AppProshopFeatures.Management);
        var proOrderCreate = proOrderRoot.AddChild(MultiTenancyPermissions.AppProOrders.Create, L("Permission:MiniAppProOrders.Create"));
        proOrderCreate.MultiTenancySide = MultiTenancySides.Tenant; proOrderCreate.RequireFeatures(AppProshopFeatures.Management);
        var proOrderEdit = proOrderRoot.AddChild(MultiTenancyPermissions.AppProOrders.Edit, L("Permission:MiniAppProOrders.Edit"));
        proOrderEdit.MultiTenancySide = MultiTenancySides.Tenant; proOrderEdit.RequireFeatures(AppProshopFeatures.Management);
        var proOrderDelete = proOrderRoot.AddChild(MultiTenancyPermissions.AppProOrders.Delete, L("Permission:MiniAppProOrders.Delete"));
        proOrderDelete.MultiTenancySide = MultiTenancySides.Tenant; proOrderDelete.RequireFeatures(AppProshopFeatures.Management);

        // 4. Proshop Board
        var proOrdersBoardRoot = proGroup.AddPermission(MultiTenancyPermissions.AppProOrdersBoard.Default, L("Permission:MiniAppProOrdersBoard"));
        proOrdersBoardRoot.MultiTenancySide = MultiTenancySides.Tenant;
        proOrdersBoardRoot.RequireFeatures(AppProshopFeatures.Management);

        // ── HOST ─────────────────────────────────────────────────────────────
        var proGroupHost = context.AddGroup("MiniAppProshopHost", L("PermissionGroup:MiniAppProshopHost"));

        var proCatHostRoot = proGroupHost.AddPermission(MultiTenancyPermissions.HostAppProCategories.Default, L("Permission:MiniAppProCategories"));
        proCatHostRoot.MultiTenancySide = MultiTenancySides.Host;
        proCatHostRoot.AddChild(MultiTenancyPermissions.HostAppProCategories.Create, L("Permission:MiniAppProCategories.Create")).MultiTenancySide = MultiTenancySides.Host;
        proCatHostRoot.AddChild(MultiTenancyPermissions.HostAppProCategories.Edit,   L("Permission:MiniAppProCategories.Edit"))  .MultiTenancySide = MultiTenancySides.Host;
        proCatHostRoot.AddChild(MultiTenancyPermissions.HostAppProCategories.Delete, L("Permission:MiniAppProCategories.Delete")).MultiTenancySide = MultiTenancySides.Host;

        var proItemHostRoot = proGroupHost.AddPermission(MultiTenancyPermissions.HostAppProItems.Default, L("Permission:MiniAppProItems"));
        proItemHostRoot.MultiTenancySide = MultiTenancySides.Host;
        proItemHostRoot.AddChild(MultiTenancyPermissions.HostAppProItems.Create, L("Permission:MiniAppProItems.Create")).MultiTenancySide = MultiTenancySides.Host;
        proItemHostRoot.AddChild(MultiTenancyPermissions.HostAppProItems.Edit,   L("Permission:MiniAppProItems.Edit"))  .MultiTenancySide = MultiTenancySides.Host;
        proItemHostRoot.AddChild(MultiTenancyPermissions.HostAppProItems.Delete, L("Permission:MiniAppProItems.Delete")).MultiTenancySide = MultiTenancySides.Host;

        var proOrderHostRoot = proGroupHost.AddPermission(MultiTenancyPermissions.HostAppProOrders.Default, L("Permission:MiniAppProOrders"));
        proOrderHostRoot.MultiTenancySide = MultiTenancySides.Host;
        proOrderHostRoot.AddChild(MultiTenancyPermissions.HostAppProOrders.Create, L("Permission:MiniAppProOrders.Create")).MultiTenancySide = MultiTenancySides.Host;
        proOrderHostRoot.AddChild(MultiTenancyPermissions.HostAppProOrders.Edit,   L("Permission:MiniAppProOrders.Edit"))  .MultiTenancySide = MultiTenancySides.Host;
        proOrderHostRoot.AddChild(MultiTenancyPermissions.HostAppProOrders.Delete, L("Permission:MiniAppProOrders.Delete")).MultiTenancySide = MultiTenancySides.Host;

        var proOrdersBoardHostRoot = proGroupHost.AddPermission(MultiTenancyPermissions.HostAppProOrdersBoard.Default, L("Permission:MiniAppProOrdersBoard"));
        proOrdersBoardHostRoot.MultiTenancySide = MultiTenancySides.Host;

        #endregion

        #region Salon Beauty Permissions

        var salonBeautyGroup = context.AddGroup("SalonBeauty", L("PermissionGroup:SalonBeauty"));

        // ========== TENANT (bị ràng bởi Feature) ==========
        // TENANT Customers
        var salonCustomerTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyCustomers.Default, L("Permission:SalonBeautyCustomers"));
        salonCustomerTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonCustomerTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonCustomerTenantCreate = salonCustomerTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyCustomers.Create, L("Permission:Create"));
        salonCustomerTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonCustomerTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonCustomerTenantEdit = salonCustomerTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyCustomers.Edit, L("Permission:Edit"));
        salonCustomerTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonCustomerTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonCustomerTenantDelete = salonCustomerTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyCustomers.Delete, L("Permission:Delete"));
        salonCustomerTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonCustomerTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // TENANT ServiceCategories
        var salonCategoryTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyServiceCategories.Default, L("Permission:SalonBeautyServiceCategories"));
        salonCategoryTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonCategoryTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonCategoryTenantCreate = salonCategoryTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyServiceCategories.Create, L("Permission:Create"));
        salonCategoryTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonCategoryTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonCategoryTenantEdit = salonCategoryTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyServiceCategories.Edit, L("Permission:Edit"));
        salonCategoryTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonCategoryTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonCategoryTenantDelete = salonCategoryTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyServiceCategories.Delete, L("Permission:Delete"));
        salonCategoryTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonCategoryTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // TENANT Services
        var salonServiceTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyServices.Default, L("Permission:SalonBeautyServices"));
        salonServiceTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonServiceTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonServiceTenantCreate = salonServiceTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyServices.Create, L("Permission:Create"));
        salonServiceTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonServiceTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonServiceTenantEdit = salonServiceTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyServices.Edit, L("Permission:Edit"));
        salonServiceTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonServiceTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonServiceTenantDelete = salonServiceTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyServices.Delete, L("Permission:Delete"));
        salonServiceTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonServiceTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // TENANT Stylists
        var salonStylistTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyStylists.Default, L("Permission:SalonBeautyStylists"));
        salonStylistTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonStylistTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonStylistTenantCreate = salonStylistTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyStylists.Create, L("Permission:Create"));
        salonStylistTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonStylistTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonStylistTenantEdit = salonStylistTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyStylists.Edit, L("Permission:Edit"));
        salonStylistTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonStylistTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonStylistTenantDelete = salonStylistTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyStylists.Delete, L("Permission:Delete"));
        salonStylistTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonStylistTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // TENANT Bookings
        var salonBookingTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyBookings.Default, L("Permission:SalonBeautyBookings"));
        salonBookingTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonBookingTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonBookingTenantCreate = salonBookingTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyBookings.Create, L("Permission:Create"));
        salonBookingTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonBookingTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonBookingTenantEdit = salonBookingTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyBookings.Edit, L("Permission:Edit"));
        salonBookingTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonBookingTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonBookingTenantCheckin = salonBookingTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyBookings.Checkin, L("Permission:Checkin"));
        salonBookingTenantCheckin.MultiTenancySide = MultiTenancySides.Tenant;
        salonBookingTenantCheckin.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonBookingTenantUpdatePayment = salonBookingTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyBookings.UpdatePayment, L("Permission:UpdatePayment"));
        salonBookingTenantUpdatePayment.MultiTenancySide = MultiTenancySides.Tenant;
        salonBookingTenantUpdatePayment.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonBookingTenantCancel = salonBookingTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyBookings.Cancel, L("Permission:Cancel"));
        salonBookingTenantCancel.MultiTenancySide = MultiTenancySides.Tenant;
        salonBookingTenantCancel.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonBookingTenantDelete = salonBookingTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyBookings.Delete, L("Permission:Delete"));
        salonBookingTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonBookingTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // ========== HOST (không ràng Feature) ==========
        var salonBeautyGroupHost = context.AddGroup("SalonBeautyHost", L("PermissionGroup:SalonBeautyHost"));

        // HOST Customers
        var salonCustomerHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyCustomers.Default, L("Permission:SalonBeautyCustomers"));
        salonCustomerHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonCustomerHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyCustomers.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonCustomerHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyCustomers.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonCustomerHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyCustomers.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // HOST ServiceCategories
        var salonCategoryHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyServiceCategories.Default, L("Permission:SalonBeautyServiceCategories"));
        salonCategoryHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonCategoryHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyServiceCategories.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonCategoryHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyServiceCategories.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonCategoryHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyServiceCategories.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // HOST Services
        var salonServiceHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyServices.Default, L("Permission:SalonBeautyServices"));
        salonServiceHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonServiceHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyServices.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonServiceHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyServices.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonServiceHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyServices.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // HOST Stylists
        var salonStylistHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyStylists.Default, L("Permission:SalonBeautyStylists"));
        salonStylistHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonStylistHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyStylists.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonStylistHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyStylists.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonStylistHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyStylists.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // HOST Bookings
        var salonBookingHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyBookings.Default, L("Permission:SalonBeautyBookings"));
        salonBookingHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonBookingHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyBookings.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonBookingHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyBookings.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonBookingHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyBookings.Checkin, L("Permission:Checkin")).MultiTenancySide = MultiTenancySides.Host;
        salonBookingHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyBookings.UpdatePayment, L("Permission:UpdatePayment")).MultiTenancySide = MultiTenancySides.Host;
        salonBookingHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyBookings.Cancel, L("Permission:Cancel")).MultiTenancySide = MultiTenancySides.Host;
        salonBookingHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyBookings.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // TENANT Locations
        var salonLocationTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyLocations.Default, L("Permission:SalonBeautyLocations"));
        salonLocationTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonLocationTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonLocationTenantCreate = salonLocationTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyLocations.Create, L("Permission:Create"));
        salonLocationTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonLocationTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonLocationTenantEdit = salonLocationTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyLocations.Edit, L("Permission:Edit"));
        salonLocationTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonLocationTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonLocationTenantDelete = salonLocationTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyLocations.Delete, L("Permission:Delete"));
        salonLocationTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonLocationTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // TENANT TimeSlots
        var salonTimeSlotTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyTimeSlots.Default, L("Permission:SalonBeautyTimeSlots"));
        salonTimeSlotTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonTimeSlotTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonTimeSlotTenantCreate = salonTimeSlotTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyTimeSlots.Create, L("Permission:Create"));
        salonTimeSlotTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonTimeSlotTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonTimeSlotTenantEdit = salonTimeSlotTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyTimeSlots.Edit, L("Permission:Edit"));
        salonTimeSlotTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonTimeSlotTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonTimeSlotTenantDelete = salonTimeSlotTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyTimeSlots.Delete, L("Permission:Delete"));
        salonTimeSlotTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonTimeSlotTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // HOST Locations
        var salonLocationHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyLocations.Default, L("Permission:SalonBeautyLocations"));
        salonLocationHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonLocationHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyLocations.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonLocationHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyLocations.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonLocationHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyLocations.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // HOST TimeSlots
        var salonTimeSlotHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyTimeSlots.Default, L("Permission:SalonBeautyTimeSlots"));
        salonTimeSlotHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonTimeSlotHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyTimeSlots.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonTimeSlotHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyTimeSlots.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonTimeSlotHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyTimeSlots.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // TENANT Deposits
        var salonDepositTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyDeposits.Default, L("Permission:SalonBeautyDeposits"));
        salonDepositTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonDepositTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonDepositTenantCreate = salonDepositTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyDeposits.Create, L("Permission:Create"));
        salonDepositTenantCreate.MultiTenancySide = MultiTenancySides.Tenant;
        salonDepositTenantCreate.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonDepositTenantEdit = salonDepositTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyDeposits.Edit, L("Permission:Edit"));
        salonDepositTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonDepositTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonDepositTenantApprove = salonDepositTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyDeposits.Approve, L("Permission:Approve"));
        salonDepositTenantApprove.MultiTenancySide = MultiTenancySides.Tenant;
        salonDepositTenantApprove.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonDepositTenantCancel = salonDepositTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyDeposits.Cancel, L("Permission:Cancel"));
        salonDepositTenantCancel.MultiTenancySide = MultiTenancySides.Tenant;
        salonDepositTenantCancel.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonDepositTenantDelete = salonDepositTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyDeposits.Delete, L("Permission:Delete"));
        salonDepositTenantDelete.MultiTenancySide = MultiTenancySides.Tenant;
        salonDepositTenantDelete.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // TENANT LoyaltyConfig
        var salonLoyaltyConfigTenantRoot = salonBeautyGroup.AddPermission(MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Default, L("Permission:SalonBeautyLoyaltyConfig"));
        salonLoyaltyConfigTenantRoot.MultiTenancySide = MultiTenancySides.Tenant;
        salonLoyaltyConfigTenantRoot.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);
        var salonLoyaltyConfigTenantEdit = salonLoyaltyConfigTenantRoot.AddChild(MultiTenancyPermissions.SalonBeautyLoyaltyConfig.Edit, L("Permission:Edit"));
        salonLoyaltyConfigTenantEdit.MultiTenancySide = MultiTenancySides.Tenant;
        salonLoyaltyConfigTenantEdit.RequireFeatures(Features.SalonBeauty.SalonBeautyFeatures.Management);

        // HOST Deposits
        var salonDepositHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyDeposits.Default, L("Permission:SalonBeautyDeposits"));
        salonDepositHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonDepositHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyDeposits.Create, L("Permission:Create")).MultiTenancySide = MultiTenancySides.Host;
        salonDepositHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyDeposits.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;
        salonDepositHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyDeposits.Approve, L("Permission:Approve")).MultiTenancySide = MultiTenancySides.Host;
        salonDepositHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyDeposits.Cancel, L("Permission:Cancel")).MultiTenancySide = MultiTenancySides.Host;
        salonDepositHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyDeposits.Delete, L("Permission:Delete")).MultiTenancySide = MultiTenancySides.Host;

        // HOST LoyaltyConfig
        var salonLoyaltyConfigHostRoot = salonBeautyGroupHost.AddPermission(MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Default, L("Permission:SalonBeautyLoyaltyConfig"));
        salonLoyaltyConfigHostRoot.MultiTenancySide = MultiTenancySides.Host;
        salonLoyaltyConfigHostRoot.AddChild(MultiTenancyPermissions.HostSalonBeautyLoyaltyConfig.Edit, L("Permission:Edit")).MultiTenancySide = MultiTenancySides.Host;

        #endregion
    }

    private static LocalizableString L(string name)
        => LocalizableString.Create<MultiTenancyResource>(name);
}
