namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Trạng thái đổi quà Hoa Linh (theo kết quả gọi API UrBox)
/// </summary>
public enum HlGiftExchangeStatus : byte
{
    /// <summary>Thất bại (UrBox trả lỗi / không phát hành được voucher)</summary>
    Failed = 0,

    /// <summary>Thành công (đã phát hành voucher)</summary>
    Success = 1,

    /// <summary>Đang xử lý (đã gửi yêu cầu, chờ UrBox)</summary>
    Processing = 2,

    /// <summary>Đã sử dụng</summary>
    Used = 3
}
