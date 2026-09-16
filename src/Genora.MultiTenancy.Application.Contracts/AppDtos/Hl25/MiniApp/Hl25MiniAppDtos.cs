using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.Hl25.MiniApp;

/// <summary>Envelope response chuẩn cho MiniApp API Hoa Linh 25 Năm (khớp FE: {success,data,error,message}).</summary>
public class Hl25ApiResult<T>
{
    public bool Success { get; set; }
    public T? Data { get; set; }
    public string? Error { get; set; }
    public string? Message { get; set; }

    public static Hl25ApiResult<T> Ok(T data, string? message = null)
        => new() { Success = true, Data = data, Message = message };

    public static Hl25ApiResult<T> Fail(string error, string? message = null)
        => new() { Success = false, Error = error, Message = message };
}

// ===== Cấu hình + Thể lệ =====

/// <summary>Cấu hình Mini App trả cho FE (public — không lộ field nhạy cảm).</summary>
public class Hl25MiniAppConfigDto
{
    public string? ProgramName { get; set; }
    public string? IntroductionHtml { get; set; }
    public string? Format { get; set; }
    public string? GiftDeliveryTime { get; set; }
    public string? RulesHtml { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Scope { get; set; }
    public string? OrganizerName { get; set; }
    public bool IsActive { get; set; }
}

// ===== Người tham gia =====

/// <summary>Request đăng ký/upsert người tham gia theo ZaloUserId.</summary>
public class Hl25RegisterRequest
{
    public string ZaloUserId { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool? IsFollowingOa { get; set; }
    public bool? HasConsent { get; set; }
}

/// <summary>Request cập nhật thông tin cá nhân (định danh qua zaloUserId).</summary>
public class Hl25UpdateProfileRequest
{
    public string ZaloUserId { get; set; } = null!;
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public Genora.MultiTenancy.Enums.Hl25AgeGroup AgeGroup { get; set; }
    public Genora.MultiTenancy.Enums.Hl25Gender Gender { get; set; }
    public string? ReceiveAddress { get; set; }
}

/// <summary>Thông tin người tham gia trả cho FE (profile + số liệu).</summary>
public class Hl25MeDto
{
    public Guid Id { get; set; }
    public string? ZaloUserId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public Genora.MultiTenancy.Enums.Hl25AgeGroup AgeGroup { get; set; }
    public Genora.MultiTenancy.Enums.Hl25Gender Gender { get; set; }
    public string? ReceiveAddress { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsFollowingOa { get; set; }
    public bool HasConsent { get; set; }
    public int RemainingSpinTurns { get; set; }
    public int TotalSpinTurns { get; set; }
    public int EarnedCycles { get; set; }
    public int TotalGiftsWon { get; set; }
}

// ===== Tạo thiệp (Frame) =====

/// <summary>Request tạo thiệp (ảnh đã upload sẵn ở FE, gửi URL + lời chúc).</summary>
public class Hl25CreateFrameRequest
{
    public string ZaloUserId { get; set; } = null!;
    public Guid? CampaignId { get; set; }
    public Guid? TemplateId { get; set; }
    public string ResultImageUrl { get; set; } = null!;
    public string? WishMessage { get; set; }

    /// <summary>Nếu true: server sẽ download ảnh từ ResultImageUrl, lưu với tên ZaloUserId_yyyyMMddHHmmss.ext rồi cập nhật URL mới.</summary>
    public bool RenameWithTimestamp { get; set; }
}

/// <summary>Kết quả tạo thiệp.</summary>
public class Hl25FrameResultDto
{
    public Guid FrameCreationId { get; set; }
    public string ResultImageUrl { get; set; } = null!;
    public string? ShareLink { get; set; }
    /// <summary>Chỉ true khi tạo thiệp cấp lượt tự nhận đầu tiên; tạo thêm thiệp không cộng lượt.</summary>
    public bool TurnGranted { get; set; }
    /// <summary>Số lượt quay còn lại sau khi tạo thiệp.</summary>
    public int RemainingSpinTurns { get; set; }
    /// <summary>Số lượt tự nhận (0-2), không tính Admin cấp.</summary>
    public int EarnedCycles { get; set; }
}

/// <summary>Request xác nhận chia sẻ thiệp thành công → nhận lượt tự động thứ hai.</summary>
public class Hl25ShareFrameRequest
{
    public string ZaloUserId { get; set; } = null!;
    public Guid FrameCreationId { get; set; }
    public Genora.MultiTenancy.Enums.Hl25SharePlatform SharePlatform { get; set; }
}

/// <summary>Kết quả sau khi chia sẻ (báo có cộng lượt hay không + số lượt hiện tại).</summary>
public class Hl25ShareResultDto
{
    public bool TurnGranted { get; set; }
    public int RemainingSpinTurns { get; set; }
    public int EarnedCycles { get; set; }
    public string? Message { get; set; }
}

// ===== Vòng quay =====

/// <summary>Ô quay trả cho FE (không lộ tỷ lệ trúng WinRate).</summary>
public class Hl25MiniAppWheelSlotDto
{
    public Guid Id { get; set; }
    public string? Label { get; set; }
    /// <summary>Ảnh hiển thị trên ô (full URL): ảnh riêng ô → WheelImageUrl của quà → ImageUrl cũ.</summary>
    public string? SlotImageUrl { get; set; }
    /// <summary>Ảnh vòng quay cấu hình tại kho quà (full URL), null nếu chưa cấu hình.</summary>
    public string? WheelImageUrl { get; set; }
    /// <summary>Ảnh thông tin quà khi trúng (full URL), độc lập với ảnh vòng quay.</summary>
    public string? GiftImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorHex { get; set; }

    /// <summary>Quà gán cho ô (null = ô "Chúc may mắn").</summary>
    public Guid? GiftId { get; set; }
    /// <summary>Tên quà gán cho ô (mapping từ kho quà).</summary>
    public string? GiftName { get; set; }
    /// <summary>Mô tả quà (mapping từ kho quà).</summary>
    public string? GiftDescription { get; set; }
    /// <summary>Ô này có phải ô trúng quà không (đã gán quà). false = ô "Chúc may mắn".</summary>
    public bool IsGift { get; set; }
}

/// <summary>Cấu hình vòng quay trả cho FE + số lượt còn lại của người chơi.</summary>
public class Hl25MiniAppWheelDto
{
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? PointerImageUrl { get; set; }
    public bool IsActive { get; set; }
    public int RemainingSpinTurns { get; set; }
    public List<Hl25MiniAppWheelSlotDto> Slots { get; set; } = new();
}

/// <summary>Request thực hiện quay.</summary>
public class Hl25SpinRequest
{
    public string ZaloUserId { get; set; } = null!;
}

/// <summary>Kết quả quay.</summary>
public class Hl25SpinResultDto
{
    public bool Won { get; set; }
    public Guid? SpinLogId { get; set; }
    public Guid? SlotId { get; set; }
    public Guid? GiftId { get; set; }
    public string? GiftName { get; set; }
    public string? GiftImageUrl { get; set; }
    public int RemainingSpinTurns { get; set; }

    // ===== Cờ FE (Delta 2026-09) — giúp FE quyết định nút trên màn kết quả =====

    /// <summary>True khi EarnedCycles = 1: đã có lượt tạo thiệp và chưa nhận lượt chia sẻ.</summary>
    public bool CanShareForMoreTurn { get; set; }

    /// <summary>Số lượt tự nhận (0-2), không tính Admin cấp.</summary>
    public int EarnedCycles { get; set; }

    /// <summary>Tổng số quà đã trúng (dùng cho luật "tối đa trúng 1 lần").</summary>
    public int TotalGiftsWon { get; set; }

    /// <summary>Người dùng đã từng trúng quà trước lượt hiện tại (TotalGiftsWon &gt; 0 trước khi quay). Lượt này sẽ bị ép trượt.</summary>
    public bool HasWonBefore { get; set; }
}

// ===== Lịch sử nhận quà =====

/// <summary>Một dòng quà đã trúng của người chơi.</summary>
public class Hl25MyGiftDto
{
    public Guid SpinLogId { get; set; }
    public Guid? GiftId { get; set; }
    public string? GiftName { get; set; }
    public string? GiftImageUrl { get; set; }
    public DateTime SpinTime { get; set; }
    public Genora.MultiTenancy.Enums.Hl25RewardStatus RewardStatus { get; set; }
    public DateTime? DeliveredTime { get; set; }
}

// ===== (Delta 2026-09) Upload ảnh =====

/// <summary>Kết quả upload ảnh — trả về URL đầy đủ (endpoint + path).</summary>
public class Hl25UploadImageResultDto
{
    /// <summary>Đường dẫn đầy đủ tới ảnh (VD https://host/uploads/hl25/host/abc.png).</summary>
    public string Url { get; set; } = null!;
}

// ===== (Delta 2026-09) Frame — Chiến dịch / Mẫu Frame / Lịch sử tạo ảnh =====

/// <summary>Chiến dịch ghép ảnh (public — FE hiển thị danh sách đợt ghép ảnh).</summary>
public class Hl25FrameCampaignPublicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    /// <summary>Trạng thái chiến dịch (0=Draft,1=Active,2=Paused,3=Ended).</summary>
    public Genora.MultiTenancy.Enums.Hl25CampaignStatus Status { get; set; }
    /// <summary>Số mẫu frame đang bật thuộc chiến dịch.</summary>
    public int TemplateCount { get; set; }
}

/// <summary>Mẫu frame/khung ảnh (public — FE cho người dùng chọn ảnh local ướm vào khung).</summary>
public class Hl25FrameTemplatePublicDto
{
    public Guid Id { get; set; }
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = null!;
    /// <summary>Ảnh khung frame (PNG khung trong suốt) — FE ghép ảnh người dùng vào.</summary>
    public string ImageUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
}

/// <summary>Một dòng lịch sử tạo ảnh của người chơi (public).</summary>
public class Hl25FrameCreationPublicDto
{
    public Guid Id { get; set; }
    public Guid? CampaignId { get; set; }
    public Guid? TemplateId { get; set; }
    public string ResultImageUrl { get; set; } = null!;
    public string? WishMessage { get; set; }
    public string? ShareLink { get; set; }
    /// <summary>Nền tảng chia sẻ (0=None,1=Zalo,2=Facebook).</summary>
    public Genora.MultiTenancy.Enums.Hl25SharePlatform SharePlatform { get; set; }
    public DateTime? ShareTime { get; set; }
    public DateTime CreatedTime { get; set; }
}

// ===== (Delta 2026-09) Wheel — Kho quà / Lịch sử nhận lượt / Lịch sử lượt quay =====

/// <summary>Quà tặng trong kho (public — FE hiển thị thông tin quà khi trúng).</summary>
public class Hl25GiftPublicDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public decimal? Value { get; set; }
    /// <summary>Trạng thái (0=Available,1=OutOfStock,2=Disabled).</summary>
    public Genora.MultiTenancy.Enums.Hl25GiftStatus Status { get; set; }
}

/// <summary>Một dòng lịch sử NHẬN lượt quay của người chơi (public).</summary>
public class Hl25SpinTurnLogPublicDto
{
    public Guid Id { get; set; }
    /// <summary>Nguồn cộng lượt (1=ShareZalo,2=ShareFacebook,3=AdminGrant,4=Other).</summary>
    public Genora.MultiTenancy.Enums.Hl25SpinTurnSource Source { get; set; }
    public int TurnsAdded { get; set; }
    public string? Note { get; set; }
    public DateTime GrantedTime { get; set; }
}

/// <summary>Một dòng lịch sử lượt quay ĐÃ THỰC HIỆN của người chơi (public).</summary>
public class Hl25SpinLogPublicDto
{
    public Guid Id { get; set; }
    public Guid? GiftId { get; set; }
    public string? GiftName { get; set; }
    public string? GiftImageUrl { get; set; }
    public DateTime SpinTime { get; set; }
    /// <summary>Trạng thái trao thưởng (0=Pending,1=Won,2=NotWon,3=Delivered,4=Cancelled).</summary>
    public Genora.MultiTenancy.Enums.Hl25RewardStatus RewardStatus { get; set; }
    public DateTime? DeliveredTime { get; set; }
    /// <summary>Có trúng quà không (RewardStatus là Won hoặc Delivered).</summary>
    public bool Won { get; set; }
}
