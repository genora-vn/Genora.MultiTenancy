namespace Genora.MultiTenancy.Permissions;

public static class MultiTenancyPermissions
{
    public const string GroupName = "MultiTenancy";

    #region Thêm permission cho tính năng quản trị AppSettings
    public static class AppSettings
    {
        public const string Default = GroupName + ".AppSettings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    // Host-side (không ràng Feature)
    public static class HostAppSettings
    {
        public const string Default = GroupName + ".HostAppSettings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppCustomerTypes
    public static class AppCustomerTypes
    {
        public const string Default = GroupName + ".AppCustomerTypes";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCustomerTypes
    {
        public const string Default = GroupName + ".HostAppCustomerTypes";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppGolfCourses
    public static class AppGolfCourses
    {
        public const string Default = GroupName + ".AppGolfCourses";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppGolfCourses
    {
        public const string Default = GroupName + ".HostAppGolfCourses";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppMembershipTiers
    public static class AppMembershipTiers
    {
        public const string Default = GroupName + ".AppMembershipTiers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppMembershipTiers
    {
        public const string Default = GroupName + ".HostAppMembershipTiers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppCustomers
    public static class AppCustomers
    {
        public const string Default = GroupName + ".AppCustomers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCustomers
    {
        public const string Default = GroupName + ".HostAppCustomers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppCalendarSlots
    public static class AppCalendarSlots
    {
        public const string Default = GroupName + ".AppCalendarSlots";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCalendarSlots
    {
        public const string Default = GroupName + ".HostAppCalendarSlots";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị News
    public static class AppNews
    {
        public const string Default = GroupName + ".AppNews";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppNews
    {
        public const string Default = GroupName + ".HostAppNews";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị Bookings
    public static class AppBookings
    {
        public const string Default = GroupName + ".AppBookings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppBookings
    {
        public const string Default = GroupName + ".HostAppBookings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppZaloAuths, AppZaloLogs
    public static class AppZaloAuths
    {
        public const string Default = GroupName + ".AppZaloAuths";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    public static class HostAppZaloAuths
    {
        public const string Default = GroupName + ".HostAppZaloAuths";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppZaloLogs
    {
        public const string Default = GroupName + ".AppZaloLogs";
    }

    public static class HostAppZaloLogs
    {
        public const string Default = GroupName + ".HostAppZaloLogs";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị PromotionType
    public static class AppPromotionType
    {
        public const string Default = "MultiTenancy.AppPromotionType";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppPromotionType
    {
        public const string Default = "MultiTenancy.HostAppPromotionType";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị PromotionPolicy
    public static class AppPromotionPolicies
    {
        public const string Default = GroupName + ".AppPromotionPolicies";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppPromotionPolicies
    {
        public const string Default = GroupName + ".HostAppPromotionPolicies";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppSpecialDates
    public static class AppSpecialDates
    {
        public const string Default = GroupName + ".AppSpecialDates";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppSpecialDates
    {
        public const string Default = GroupName + ".HostAppSpecialDates";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị AppEmails
    public static class AppEmails
    {
        public const string Default = GroupName + ".AppEmails";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Send = Default + ".Send";
        public const string Resend = Default + ".Resend";
    }

    public static class HostAppEmails
    {
        public const string Default = GroupName + ".HostAppEmails";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Send = Default + ".Send";
        public const string Resend = Default + ".Resend";
    }
    #endregion

    public static class AppHomePageConfigs
    {
        public const string Default = GroupName + ".AppHomePageConfigs";
        public const string Edit = Default + ".Edit";
    }

    public static class HostAppHomePageConfigs
    {
        public const string Default = GroupName + ".HostAppHomePageConfigs";
        public const string Edit = Default + ".Edit";
    }

    #region Thêm permission cho tính năng quản trị Fnb
    public static class AppFnbCategories
    {
        public const string Default = GroupName + ".AppFnbCategories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppFnbCategories
    {
        public const string Default = GroupName + ".HostAppFnbCategories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppFnbKitchenBoard
    {
        public const string Default = GroupName + ".AppFnbKitchenBoard";
    }

    public static class HostAppFnbKitchenBoard
    {
        public const string Default = GroupName + ".HostAppFnbKitchenBoard";
    }

    public static class AppFnbItems
    {
        public const string Default = GroupName + ".AppFnbItems";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppFnbItems
    {
        public const string Default = GroupName + ".HostAppFnbItems";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppFnbOrders
    {
        public const string Default = GroupName + ".AppFnbOrders";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppFnbOrders
    {
        public const string Default = GroupName + ".HostAppFnbOrders";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    
    #endregion

    #region Proshop permissions

    public static class AppProCategories
    {
        public const string Default = GroupName + ".AppProCategories";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    public static class HostAppProCategories
    {
        public const string Default = GroupName + ".HostAppProCategories";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    public static class AppProItems
    {
        public const string Default = GroupName + ".AppProItems";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    public static class HostAppProItems
    {
        public const string Default = GroupName + ".HostAppProItems";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    public static class AppProOrders
    {
        public const string Default = GroupName + ".AppProOrders";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    public static class HostAppProOrders
    {
        public const string Default = GroupName + ".HostAppProOrders";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    public static class AppProOrdersBoard
    {
        public const string Default = GroupName + ".AppProOrdersBoard";
    }

    public static class HostAppProOrdersBoard
    {
        public const string Default = GroupName + ".HostAppProOrdersBoard";
    }

    #endregion

    #region PaymentConfiguration

    public static class AppPaymentConfigurations
    {
        public const string Default = GroupName + ".AppPaymentConfigurations";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    public static class HostAppPaymentConfigurations
    {
        public const string Default = GroupName + ".HostAppPaymentConfigurations";
        public const string Create  = Default + ".Create";
        public const string Edit    = Default + ".Edit";
        public const string Delete  = Default + ".Delete";
    }

    #endregion

    #region Salon Beauty Permissions
    public static class SalonBeautyCustomers
    {
        public const string Default = GroupName + ".SalonBeautyCustomers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostSalonBeautyCustomers
    {
        public const string Default = GroupName + ".HostSalonBeautyCustomers";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class SalonBeautyServiceCategories
    {
        public const string Default = GroupName + ".SalonBeautyServiceCategories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostSalonBeautyServiceCategories
    {
        public const string Default = GroupName + ".HostSalonBeautyServiceCategories";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class SalonBeautyServices
    {
        public const string Default = GroupName + ".SalonBeautyServices";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostSalonBeautyServices
    {
        public const string Default = GroupName + ".HostSalonBeautyServices";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class SalonBeautyStylists
    {
        public const string Default = GroupName + ".SalonBeautyStylists";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostSalonBeautyStylists
    {
        public const string Default = GroupName + ".HostSalonBeautyStylists";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class SalonBeautyBookings
    {
        public const string Default = GroupName + ".SalonBeautyBookings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Checkin = Default + ".Checkin";
        public const string UpdatePayment = Default + ".UpdatePayment";
        public const string Cancel = Default + ".Cancel";
    }

    public static class HostSalonBeautyBookings
    {
        public const string Default = GroupName + ".HostSalonBeautyBookings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Checkin = Default + ".Checkin";
        public const string UpdatePayment = Default + ".UpdatePayment";
        public const string Cancel = Default + ".Cancel";
    }

    public static class SalonBeautyLocations
    {
        public const string Default = GroupName + ".SalonBeautyLocations";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostSalonBeautyLocations
    {
        public const string Default = GroupName + ".HostSalonBeautyLocations";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class SalonBeautyTimeSlots
    {
        public const string Default = GroupName + ".SalonBeautyTimeSlots";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostSalonBeautyTimeSlots
    {
        public const string Default = GroupName + ".HostSalonBeautyTimeSlots";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class SalonBeautyDeposits
    {
        public const string Default = GroupName + ".SalonBeautyDeposits";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
        public const string Cancel = Default + ".Cancel";
    }

    public static class HostSalonBeautyDeposits
    {
        public const string Default = GroupName + ".HostSalonBeautyDeposits";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
        public const string Approve = Default + ".Approve";
        public const string Cancel = Default + ".Cancel";
    }

    public static class SalonBeautyLoyaltyConfig
    {
        public const string Default = GroupName + ".SalonBeautyLoyaltyConfig";
        public const string Edit = Default + ".Edit";
    }

    public static class HostSalonBeautyLoyaltyConfig
    {
        public const string Default = GroupName + ".HostSalonBeautyLoyaltyConfig";
        public const string Edit = Default + ".Edit";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị Documents (tài liệu hướng dẫn)
    public static class AppDocuments
    {
        public const string Default = GroupName + ".AppDocuments";
    }

    public static class HostAppDocuments
    {
        public const string Default = GroupName + ".HostAppDocuments";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

    #region Thêm permission cho tính năng quản trị Caddie
    public static class AppCaddies
    {
        public const string Default = GroupName + ".AppCaddies";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCaddies
    {
        public const string Default = GroupName + ".HostAppCaddies";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppCaddieSkills
    {
        public const string Default = GroupName + ".AppCaddieSkills";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCaddieSkills
    {
        public const string Default = GroupName + ".HostAppCaddieSkills";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppCaddieBookings
    {
        public const string Default = GroupName + ".AppCaddieBookings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCaddieBookings
    {
        public const string Default = GroupName + ".HostAppCaddieBookings";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppCaddieSchedules
    {
        public const string Default = GroupName + ".AppCaddieSchedules";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCaddieSchedules
    {
        public const string Default = GroupName + ".HostAppCaddieSchedules";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppCaddieRatings
    {
        public const string Default = GroupName + ".AppCaddieRatings";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppCaddieRatings
    {
        public const string Default = GroupName + ".HostAppCaddieRatings";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppLanguages
    {
        public const string Default = GroupName + ".AppLanguages";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppLanguages
    {
        public const string Default = GroupName + ".HostAppLanguages";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppCaddieDashboard
    {
        public const string Default = GroupName + ".AppCaddieDashboard";
    }

    public static class HostAppCaddieDashboard
    {
        public const string Default = GroupName + ".HostAppCaddieDashboard";
    }

    public static class AppCaddieReports
    {
        public const string Default = GroupName + ".AppCaddieReports";
    }

    public static class HostAppCaddieReports
    {
        public const string Default = GroupName + ".HostAppCaddieReports";
    }
    #endregion

    #region Hoa Linh

    public static class AppHlProducts
    {
        public const string Default = GroupName + ".AppHlProducts";
    }

    public static class HostAppHlProducts
    {
        public const string Default = GroupName + ".HostAppHlProducts";
    }

    public static class AppHlCustomers
    {
        public const string Default = GroupName + ".AppHlCustomers";
    }

    public static class HostAppHlCustomers
    {
        public const string Default = GroupName + ".HostAppHlCustomers";
    }

    public static class AppHlOrders
    {
        public const string Default = GroupName + ".AppHlOrders";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppHlOrders
    {
        public const string Default = GroupName + ".HostAppHlOrders";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppHlLoyalty
    {
        public const string Default = GroupName + ".AppHlLoyalty";
    }

    public static class HostAppHlLoyalty
    {
        public const string Default = GroupName + ".HostAppHlLoyalty";
    }

    public static class AppHlGiftExchange
    {
        public const string Default = GroupName + ".AppHlGiftExchange";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class HostAppHlGiftExchange
    {
        public const string Default = GroupName + ".HostAppHlGiftExchange";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    public static class AppHlDashboard
    {
        public const string Default = GroupName + ".AppHlDashboard";
    }

    public static class HostAppHlDashboard
    {
        public const string Default = GroupName + ".HostAppHlDashboard";
    }

    public static class AppHlApiLogs
    {
        public const string Default = GroupName + ".AppHlApiLogs";
    }

    public static class HostAppHlApiLogs
    {
        public const string Default = GroupName + ".HostAppHlApiLogs";
    }

    #endregion

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}