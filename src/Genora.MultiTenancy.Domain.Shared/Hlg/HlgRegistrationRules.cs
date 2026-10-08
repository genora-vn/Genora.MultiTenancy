using System;
using System.Text.RegularExpressions;

namespace Genora.MultiTenancy.Hlg;

public static class HlgRegistrationRules
{
    public const int MaxLinkedAccounts = 5; // Owner + four employees.
    public const string DmsCustomerNotFound = "Số điện thoại của bạn chưa có trong hệ thống. Xin vui lòng liên hệ Hotline 0977872631 để được hỗ trợ";
    public const string OwnerRequired = "Vui lòng thông báo chủ nhà thuốc đăng ký tài khoản trước khi bạn thực hiện đăng ký.";
    public const string AccountLimitReached = "Số lượng tài khoản liên kết đã vượt quá quy định của Nhà thuốc.";

    public static string? NormalizePhone(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        var phone = Regex.Replace(value.Trim(), @"[\s.()-]", "");
        if (phone.StartsWith("+84", StringComparison.Ordinal)) phone = "0" + phone[3..];
        else if (phone.StartsWith("84", StringComparison.Ordinal)) phone = "0" + phone[2..];
        return phone;
    }

    public static bool IsValidPhone(string? phone) => phone != null && Regex.IsMatch(phone, @"^0\d{9,10}$");

    public static string[] PhoneAliases(string phone) => new[] { phone, "84" + phone[1..], "+84" + phone[1..] };
}
