namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Đơn vị lô điểm thưởng Hoa Linh — phân biệt lô là điểm hay tiền
/// </summary>
public enum HlPointUnit : byte
{
    /// <summary>Điểm (accumulatedPoints) — cộng vào BonusPoint</summary>
    Point = 1,

    /// <summary>Tiền (accumulatedSales) — cộng vào BonusAmount</summary>
    Amount = 2
}
