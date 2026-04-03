namespace Genora.MultiTenancy.Enums;

public enum PaymentOrderStatus : byte
{
    /// <summary>Đơn hàng vừa tạo, chờ thanh toán</summary>
    Pending = 0,

    /// <summary>Thanh toán thành công (Zalo callback resultCode = 1)</summary>
    Success = 1,

    /// <summary>Thanh toán thất bại (Zalo callback resultCode = -1)</summary>
    Failed = 2,

    /// <summary>Giao dịch bị huỷ bởi người dùng</summary>
    Cancelled = 3
}
