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
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public string? TvcUrl { get; set; }
    public string? TvcHtml { get; set; }
    public string? RulesHtml { get; set; }
    public string? GamePlayHtml { get; set; }
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
    public DateTime? BirthDate { get; set; }
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
    public DateTime? BirthDate { get; set; }
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
}

/// <summary>Kết quả tạo thiệp.</summary>
public class Hl25FrameResultDto
{
    public Guid FrameCreationId { get; set; }
    public string ResultImageUrl { get; set; } = null!;
    public string? ShareLink { get; set; }
}

/// <summary>Request xác nhận chia sẻ thiệp thành công → cộng lượt (theo chu kỳ).</summary>
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
    public string? SlotImageUrl { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorHex { get; set; }
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
