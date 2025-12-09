namespace Genora.MultiTenancy.Permissions;

public static class MultiTenancyPermissions
{
    public const string GroupName = "MultiTenancy";

    #region Thêm permission cho tính năng quản trị Books
    public static class Books
    {
        public const string Default = GroupName + ".Books";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }

    // Host-side (không ràng Feature)
    public static class HostBooks
    {
        public const string Default = GroupName + ".HostBooks";
        public const string Create = Default + ".Create";
        public const string Edit = Default + ".Edit";
        public const string Delete = Default + ".Delete";
    }
    #endregion

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

    //Add your own permission names. Example:
    //public const string MyPermission1 = GroupName + ".MyPermission1";
}
