using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace Genora.MultiTenancy.Hlg;

public static class HlgRegistrationRules
{
    public const int MaxLinkedAccounts = 5; // Owner + four employees.
    public const string DmsCustomerNotFound = "Số điện thoại của bạn chưa có trong hệ thống. Xin vui lòng liên hệ Hotline 0977872631 để được hỗ trợ";
    public const string OwnerRequired = "Vui lòng thông báo chủ nhà thuốc đăng ký tài khoản trước khi bạn thực hiện đăng ký.";
    public const string AccountLimitReached = "Số lượng tài khoản liên kết đã vượt quá quy định của Nhà thuốc.";
    public const string ChannelNotAllowed = "Cảm ơn Quý khách quan tâm chương trình. Rất tiếc, chương trình hiện tại đang áp dụng cho Khách hàng thuộc Kênh Nhà Thuốc";

    /// <summary>Kênh khách hàng DMS mặc định được phép đăng ký: chỉ Kênh Nhà Thuốc (OTC).</summary>
    public static readonly string[] DefaultAllowedCustomerChannels = { "OTC" };

    /// <summary>Giá trị cấu hình mở cho mọi kênh khách hàng.</summary>
    public const string AllowAllChannelsToken = "*";

    /// <summary>
    /// Chuẩn hóa danh sách kênh được phép từ cấu hình. Fail-closed: cấu hình thiếu/rỗng
    /// trả về <see cref="DefaultAllowedCustomerChannels"/> thay vì mở cho mọi kênh.
    /// </summary>
    public static string[] NormalizeAllowedChannels(string[]? configured)
    {
        var values = (configured ?? Array.Empty<string>())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        return values.Length == 0 ? DefaultAllowedCustomerChannels : values;
    }

    /// <summary>Danh sách được phép có mở cho mọi kênh (chứa "*") hay không.</summary>
    public static bool AllowsAllChannels(IReadOnlyCollection<string> allowedChannels)
        => allowedChannels.Any(x => string.Equals(x, AllowAllChannelsToken, StringComparison.Ordinal));

    /// <summary>Kênh khách hàng DMS có thuộc danh sách được phép hay không. Null/rỗng/không khớp → false.</summary>
    public static bool IsChannelAllowed(string? channel, IReadOnlyCollection<string> allowedChannels)
    {
        if (AllowsAllChannels(allowedChannels)) return true;
        if (string.IsNullOrWhiteSpace(channel)) return false;
        var value = channel.Trim();
        return allowedChannels.Any(x => string.Equals(x, value, StringComparison.OrdinalIgnoreCase));
    }

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
