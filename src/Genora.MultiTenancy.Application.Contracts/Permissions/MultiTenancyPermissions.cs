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
    #endregion

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}