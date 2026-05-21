namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái hoạt động của 1 time slot lịch làm việc của stylist.
/// Theo SRS: AVAILABLE / FULL / OFF.
/// </summary>
public enum SalonBeautyTimeSlotStatus : byte
{
    /// <summary>
    /// OFF - stylist nghỉ, không nhận đặt lịch trong khung giờ này.
    /// </summary>
    Off = 0,

    /// <summary>
    /// ON / AVAILABLE - stylist đang nhận khách trong khung giờ này (booked_count &lt; capacity).
    /// </summary>
    On = 1,

    /// <summary>
    /// FULL - khung giờ đã có khách đặt full (booked_count &gt;= capacity).
    /// </summary>
    Full = 2
}
