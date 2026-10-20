namespace Genora.MultiTenancy.Hlg;

/// <summary>
/// Cấu hình điều kiện đăng ký HLG.
/// Bind từ section "HlgRegistration" trong appsettings.json (tùy chọn).
/// </summary>
public sealed class HlgRegistrationOptions
{
    public const string SectionName = "HlgRegistration";

    /// <summary>
    /// Danh sách kênh khách hàng DMS (cust_channel) được phép đăng ký.
    /// Null/rỗng → mặc định <see cref="HlgRegistrationRules.DefaultAllowedCustomerChannels"/> (chỉ "OTC").
    /// Chứa "*" (<see cref="HlgRegistrationRules.AllowAllChannelsToken"/>) → mở cho mọi kênh.
    /// </summary>
    public string[]? AllowedCustomerChannels { get; set; }

    /// <summary>
    /// Override thông báo khi không có chi nhánh nào thuộc kênh được phép.
    /// Null/rỗng → dùng <see cref="HlgRegistrationRules.ChannelNotAllowed"/>.
    /// </summary>
    public string? ChannelRestrictedMessage { get; set; }
}
