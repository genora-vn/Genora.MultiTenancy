namespace Genora.MultiTenancy.Enums;

/// <summary>
/// Module: Dược Phẩm Hoa Linh 25 Năm (schema DB "hl25").
/// Tập hợp enum dùng chung cho các entity của chương trình kỷ niệm 25 năm.
/// </summary>

/// <summary>Trạng thái quà tặng trong kho (Hl25Gift).</summary>
public enum Hl25GiftStatus : byte
{
    /// <summary>Còn hàng — có thể trao thưởng.</summary>
    Available = 0,

    /// <summary>Hết hàng — RemainingQuantity &lt;= 0.</summary>
    OutOfStock = 1,

    /// <summary>Vô hiệu hóa — không tham gia vòng quay.</summary>
    Disabled = 2
}

/// <summary>
/// Nguồn cộng lượt quay (Hl25SpinTurnLog).
/// Tạo thiệp nhận lượt đầu (Other); chia sẻ nhận lượt thứ hai (ShareZalo/ShareFacebook).
/// AdminGrant không tính vào trần hai lượt tự nhận.
/// </summary>
public enum Hl25SpinTurnSource : byte
{
    /// <summary>Cộng lượt do chia sẻ thiệp qua Zalo thành công.</summary>
    ShareZalo = 1,

    /// <summary>Cộng lượt do chia sẻ thiệp qua Facebook thành công.</summary>
    ShareFacebook = 2,

    /// <summary>Admin cấp lượt thủ công.</summary>
    AdminGrant = 3,

    /// <summary>Nguồn khác.</summary>
    Other = 4
}

/// <summary>Trạng thái trao thưởng của một lượt quay (Hl25SpinLog).</summary>
public enum Hl25RewardStatus : byte
{
    /// <summary>Chờ xử lý.</summary>
    Pending = 0,

    /// <summary>Trúng quà — chờ Admin xác nhận trao.</summary>
    Won = 1,

    /// <summary>Không trúng (ô "Chúc may mắn").</summary>
    NotWon = 2,

    /// <summary>Đã trao thưởng (Admin xác nhận).</summary>
    Delivered = 3,

    /// <summary>Đã hủy.</summary>
    Cancelled = 4
}

/// <summary>Trạng thái chiến dịch ghép ảnh Frame (Hl25FrameCampaign).</summary>
public enum Hl25CampaignStatus : byte
{
    /// <summary>Nháp — chưa chạy.</summary>
    Draft = 0,

    /// <summary>Đang chạy.</summary>
    Active = 1,

    /// <summary>Tạm dừng.</summary>
    Paused = 2,

    /// <summary>Đã kết thúc.</summary>
    Ended = 3
}

/// <summary>Nền tảng chia sẻ thiệp (Hl25FrameCreation).</summary>
public enum Hl25SharePlatform : byte
{
    /// <summary>Chưa chia sẻ.</summary>
    None = 0,

    /// <summary>Chia sẻ qua Zalo.</summary>
    Zalo = 1,

    /// <summary>Chia sẻ qua Facebook.</summary>
    Facebook = 2
}

/// <summary>Giới tính người tham gia (Hl25Participant).</summary>
public enum Hl25Gender : byte
{
    /// <summary>Không xác định.</summary>
    Unknown = 0,

    /// <summary>Nam.</summary>
    Male = 1,

    /// <summary>Nữ.</summary>
    Female = 2,

    /// <summary>Khác.</summary>
    Other = 3
}

/// <summary>
/// Nhóm tuổi người tham gia (Hl25Participant).
/// Delta 2026-09: màn "Thông tin nhận quà" (Figma FE) dùng nhóm tuổi thay ngày sinh.
/// </summary>
public enum Hl25AgeGroup : byte
{
    /// <summary>Không xác định.</summary>
    Unknown = 0,

    /// <summary>18 - 25 tuổi.</summary>
    Age18To25 = 1,

    /// <summary>26 - 35 tuổi.</summary>
    Age26To35 = 2,

    /// <summary>36 - 44 tuổi.</summary>
    Age36To44 = 3
}
