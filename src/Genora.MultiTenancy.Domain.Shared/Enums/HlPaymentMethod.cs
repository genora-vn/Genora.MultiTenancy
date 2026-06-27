namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Phương thức thanh toán Hoa Linh
/// </summary>
public enum HlPaymentMethod : byte
{
    /// <summary>Thanh toán tiền mặt (COD)</summary>
    Cash = 1,

    /// <summary>Chuyển khoản ngân hàng</summary>
    BankTransfer = 2
}
