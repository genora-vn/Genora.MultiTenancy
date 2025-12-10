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

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}