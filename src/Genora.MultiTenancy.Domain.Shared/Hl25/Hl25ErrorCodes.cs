namespace Genora.MultiTenancy.Hl25;

/// <summary>
/// Mã lỗi chuẩn cho MiniApp API "Dược Phẩm Hoa Linh 25 Năm".
/// FE dùng field `error` trong Hl25ApiResult để nhận diện và hiển thị thông báo phù hợp.
/// Quy ước: mọi mã bắt đầu bằng "Hl25:".
/// </summary>
public static class Hl25ErrorCodes
{
    public const string InvalidSharePlatform = "Hl25:InvalidSharePlatform";
    /// <summary>Thiếu ZaloUserId trong request.</summary>
    public const string MissingZaloUserId = "Hl25:MissingZaloUserId";

    /// <summary>Người dùng chưa đăng ký chương trình.</summary>
    public const string ParticipantNotFound = "Hl25:ParticipantNotFound";

    /// <summary>Thiếu ảnh thiệp khi tạo frame.</summary>
    public const string FrameImageRequired = "Hl25:FrameImageRequired";

    /// <summary>Lời chúc vượt quá độ dài tối đa.</summary>
    public const string WishTooLong = "Hl25:WishTooLong";

    /// <summary>Không tìm thấy thiệp.</summary>
    public const string FrameNotFound = "Hl25:FrameNotFound";

    /// <summary>Thiệp không thuộc về người dùng này.</summary>
    public const string FrameNotOwned = "Hl25:FrameNotOwned";

    /// <summary>Người dùng đã hết lượt quay.</summary>
    public const string NoSpinTurns = "Hl25:NoSpinTurns";

    /// <summary>Vòng quay chưa được cấu hình.</summary>
    public const string WheelNotConfigured = "Hl25:WheelNotConfigured";

    /// <summary>Vòng quay đang tạm dừng.</summary>
    public const string WheelInactive = "Hl25:WheelInactive";

    /// <summary>Vòng quay chưa có ô quay.</summary>
    public const string WheelNoSlots = "Hl25:WheelNoSlots";

    /// <summary>Chương trình chưa được kích hoạt / chưa cấu hình.</summary>
    public const string ProgramInactive = "Hl25:ProgramInactive";

    /// <summary>Thiếu file ảnh khi upload.</summary>
    public const string ImageRequired = "Hl25:ImageRequired";

    /// <summary>File ảnh vượt quá dung lượng cho phép (5MB).</summary>
    public const string ImageTooLarge = "Hl25:ImageTooLarge";

    /// <summary>Upload ảnh thất bại (sai định dạng, lỗi lưu trữ...).</summary>
    public const string UploadFailed = "Hl25:UploadFailed";

    /// <summary>Lỗi không xác định.</summary>
    public const string Unknown = "Hl25:Unknown";
}
