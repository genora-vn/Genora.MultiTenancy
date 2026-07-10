namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Loại giao dịch điểm thưởng Hoa Linh (sổ cái AppHlPointTransactions)
/// </summary>
public enum HlPointTransactionType : byte
{
    /// <summary>Đổi điểm/tiền từ chiến dịch (cộng)</summary>
    Earn = 1,

    /// <summary>Tiêu điểm — đổi quà (trừ)</summary>
    Spend = 2,

    /// <summary>Hết hạn — job quét trừ (trừ)</summary>
    Expire = 3,

    /// <summary>Điều chỉnh thủ công</summary>
    Adjust = 4
}
