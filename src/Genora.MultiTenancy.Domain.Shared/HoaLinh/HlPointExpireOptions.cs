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

    /// <summary>
    /// Chỉ trừ quỹ khi lô hết hạn được TẠO TỪ mốc này trở đi.
    /// Lý do: luồng đổi quà UrBox cũ không trừ RemainingValue của lô, nên lô tạo trước
    /// bản fix vẫn giữ nguyên giá trị gốc dù khách đã tiêu. Trừ con số đó khỏi BonusAmount
    /// khi lô hết hạn sẽ xóa oan số dư hợp lệ. Lô cũ hơn mốc này chỉ được đóng lại.
    /// Null (mặc định) = không trừ quỹ cho bất kỳ lô nào, chỉ đóng lô.
    /// Đặt bằng ngày deploy bản fix.
    /// </summary>
    public DateTime? TrustBatchesFrom { get; set; }
}
