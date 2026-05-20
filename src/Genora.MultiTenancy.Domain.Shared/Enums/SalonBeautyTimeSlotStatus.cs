namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái hoạt động của 1 time slot lịch làm việc của stylist.
/// </summary>
public enum SalonBeautyTimeSlotStatus : byte
{
    /// <summary>
    /// Tắt - stylist nghỉ, không nhận đặt lịch trong khung giờ này.
    /// </summary>
    Off = 0,

    /// <summary>
    /// Bật - stylist đang nhận khách trong khung giờ này.
    /// </summary>
    On = 1,

    /// <summary>
    /// Đầy - khung giờ đã có khách đặt full, không nhận thêm.
    /// </summary>
    Full = 2
}
