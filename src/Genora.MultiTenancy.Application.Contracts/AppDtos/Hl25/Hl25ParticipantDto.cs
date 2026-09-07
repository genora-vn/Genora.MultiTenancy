using System;
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
}
