namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái giao dịch nạp tiền (DepositTransaction).
/// </summary>
public enum DepositStatus : byte
{
    /// <summary>Chờ duyệt — chưa cộng điểm vào ví khách.</summary>
    Pending = 1,

    /// <summary>Thành công — điểm đã cộng vào ví khách (immutable).</summary>
    Success = 2,

    /// <summary>Đã hủy — không cộng điểm hoặc đã rollback (immutable).</summary>
    Cancelled = 3
}

/// <summary>
/// Phương thức nạp tiền.
/// </summary>
public enum DepositPaymentMethod : byte
{
    /// <summary>Tiền mặt tại quầy.</summary>
    Cash = 1,

    /// <summary>Chuyển khoản ngân hàng.</summary>
    BankTransfer = 2,

    /// <summary>Ví điện tử (Momo/ZaloPay/...).</summary>
    EWallet = 3
}

/// <summary>
/// Loại giao dịch ví điểm thưởng.
/// </summary>
public enum LoyaltyTransactionType : byte
{
    /// <summary>Cộng điểm do nạp tiền (qua Deposit).</summary>
    Deposit = 1,

    /// <summary>Cộng điểm thưởng (manual bonus).</summary>
    Earn = 2,

    /// <summary>Trừ điểm do đổi quà / sử dụng dịch vụ.</summary>
    Redeem = 3,

    /// <summary>Điều chỉnh thủ công (admin adjust).</summary>
    Adjust = 4,

    /// <summary>Hoàn lại điểm (refund khi cancel deposit success).</summary>
    Refund = 5
}

/// <summary>
/// Loại tham chiếu của giao dịch ví điểm — dùng để trace nguồn gốc.
/// </summary>
public enum LoyaltyReferenceType : byte
{
    /// <summary>Tham chiếu đến SalonBeautyDepositTransaction.</summary>
    Deposit = 1,

    /// <summary>Tham chiếu đến SalonBeautyBooking (cộng/trừ khi sử dụng dịch vụ).</summary>
    Booking = 2,

    /// <summary>Manual — không có entity tham chiếu.</summary>
    Manual = 99
}
