namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái hoạt động của 1 time slot lịch làm việc của stylist.
/// Theo SRS: AVAILABLE / FULL / OFF / PEAK.
/// </summary>
public enum SalonBeautyTimeSlotStatus : byte
{
    /// <summary>
    /// OFF - stylist nghỉ, không nhận đặt lịch trong khung giờ này.
    /// Mini App disable slot, không cho khách chọn.
    /// </summary>
    Off = 0,

    /// <summary>
    /// ON / AVAILABLE - stylist đang nhận khách trong khung giờ này (booked_count &lt; capacity).
    /// Mini App enable slot bình thường.
    /// </summary>
    On = 1,

    /// <summary>
    /// FULL - khung giờ đã có khách đặt full (booked_count &gt;= capacity).
    /// Mini App disable slot, không cho khách chọn.
    /// </summary>
    Full = 2,

    /// <summary>
    /// PEAK HOUR - khung giờ cao điểm. Khách vẫn có thể đặt nhưng Mini App hiển thị màu đỏ
    /// để báo cho khách biết khung giờ đông người đặt.
    /// </summary>
    PeakHour = 3
}
