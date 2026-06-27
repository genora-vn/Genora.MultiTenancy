namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái đổi quà Hoa Linh
/// </summary>
public enum HlGiftExchangeStatus : byte
{
    /// <summary>Chờ xử lý</summary>
    Pending = 1,

    /// <summary>Đã duyệt</summary>
    Approved = 2,

    /// <summary>Từ chối</summary>
    Rejected = 3,

    /// <summary>Hoàn thành</summary>
    Completed = 4
}
