namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái giao hàng đơn hàng Hoa Linh
/// </summary>
public enum HlOrderDeliveryStatus : byte
{
    /// <summary>Chờ xác nhận</summary>
    PendingConfirmation = 1,

    /// <summary>Đang xử lý</summary>
    Processing = 2,

    /// <summary>Đang giao</summary>
    Delivering = 3,

    /// <summary>Hoàn thành</summary>
    Completed = 4,

    /// <summary>Đã hủy</summary>
    Cancelled = 5
}
