using System;

namespace Genora.MultiTenancy.HoaLinh;

/// <summary>
/// Cấu hình job quét điểm/tiền thưởng Hoa Linh hết hạn.
/// Bind từ section "HlPointExpire" trong appsettings.json (tùy chọn).
/// </summary>
public sealed class HlPointExpireOptions
{
    public const string SectionName = "HlPointExpire";

    public bool Enabled { get; set; } = true;

    /// <summary>Chu kỳ quét (mặc định 1 giờ)</summary>
    public TimeSpan Period { get; set; } = TimeSpan.FromHours(1);
}
