namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái thanh toán đơn hàng Hoa Linh
/// </summary>
public enum HlOrderPaymentStatus : byte
{
    /// <summary>Chưa thanh toán</summary>
    Unpaid = 1,

    /// <summary>Đã thanh toán</summary>
    Paid = 2,

    /// <summary>Công nợ</summary>
    Debt = 3
}
