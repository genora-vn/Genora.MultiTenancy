using System;
using System.Collections.Generic;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO đọc người tham gia chương trình.</summary>
public class Hl25ParticipantDto : AuditedEntityDto<Guid>
{
    public string? ZaloUserId { get; set; }
    public string? FullName { get; set; }
    public string? PhoneNumber { get; set; }
    public Hl25AgeGroup AgeGroup { get; set; }
    public Hl25Gender Gender { get; set; }
    public string? ReceiveAddress { get; set; }
    public DateTime JoinedTime { get; set; }
    public bool IsFollowingOa { get; set; }
    public bool HasConsent { get; set; }
    public DateTime? ConsentTime { get; set; }
    public int RemainingSpinTurns { get; set; }
    public int TotalSpinTurns { get; set; }
    public int EarnedCycles { get; set; }
    public int TotalGiftsWon { get; set; }
    public string? AvatarUrl { get; set; }

    /// <summary>Ảnh thiệp mới nhất (join từ FrameCreation).</summary>
    public string? LatestFrameImageUrl { get; set; }

    /// <summary>Lời chúc mới nhất (join từ FrameCreation).</summary>
    public string? LatestWishMessage { get; set; }

    /// <summary>Thời gian tạo thiệp mới nhất.</summary>
    public DateTime? LatestFrameTime { get; set; }

    /// <summary>Lịch sử vòng quay gần đây (top 5 lượt quay).</summary>
    public List<Hl25ParticipantSpinLogSummary>? RecentSpinLogs { get; set; }
}

/// <summary>Tóm tắt một lượt quay của participant (dùng trong danh sách người dùng).</summary>
public class Hl25ParticipantSpinLogSummary
{
    public DateTime SpinTime { get; set; }
    public string? GiftName { get; set; }
    public Hl25RewardStatus RewardStatus { get; set; }
}
