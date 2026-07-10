namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái lô điểm/tiền đã đổi (AppHlPointBatches)
/// </summary>
public enum HlPointBatchStatus : byte
{
    /// <summary>Còn hiệu lực (còn giá trị chưa tiêu, chưa hết hạn)</summary>
    Active = 1,

    /// <summary>Đã tiêu hết (RemainingValue = 0)</summary>
    Exhausted = 2,

    /// <summary>Hết hạn (job quét trừ phần còn lại)</summary>
    Expired = 3
}
