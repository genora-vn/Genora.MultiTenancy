using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO đọc lịch sử lượt quay (read-only + cập nhật trạng thái trao thưởng).</summary>
public class Hl25SpinLogDto : AuditedEntityDto<Guid>
{
    public Guid ParticipantId { get; set; }

    /// <summary>Họ tên người tham gia (join sang Participant để hiển thị).</summary>
    public string? ParticipantName { get; set; }

    /// <summary>SĐT người tham gia.</summary>
    public string? ParticipantPhone { get; set; }

    public Guid? WheelSlotId { get; set; }
    public Guid? GiftId { get; set; }
    public string? GiftNameSnapshot { get; set; }
    public DateTime SpinTime { get; set; }
    public Hl25RewardStatus RewardStatus { get; set; }
    public DateTime? DeliveredTime { get; set; }
    public string? ReceiverAddressSnapshot { get; set; }
}
