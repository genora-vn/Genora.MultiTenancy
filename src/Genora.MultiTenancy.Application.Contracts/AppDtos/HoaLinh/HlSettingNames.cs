using System;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Cấu hình mapping giữa ABP User và mã Sales trên DMS Hoa Linh
/// Lưu vào AppSettings với key pattern: HoaLinh:UserDsrCode:{UserId}
/// </summary>
public static class HlSettingNames
{
    public const string Prefix = "HoaLinh";

    /// <summary>
    /// Key pattern cho mapping User → DsrCode
    /// Lưu trong AppSettings: HoaLinh.UserDsrCode.{userId} = "HL00019"
    /// </summary>
    public static string GetUserDsrCodeKey(Guid userId) => $"{Prefix}.UserDsrCode.{userId}";

    /// <summary>
    /// Key lưu danh sách user IDs có role Sales (cache)
    /// </summary>
    public const string SalesRoleUserIds = Prefix + ".SalesRoleUserIds";
}
