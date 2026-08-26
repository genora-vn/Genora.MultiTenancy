using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO đọc lịch sử nhận lượt quay (read-only).</summary>
public class Hl25SpinTurnLogDto : AuditedEntityDto<Guid>
{
    public Guid ParticipantId { get; set; }

    /// <summary>Họ tên người tham gia (join sang Participant để hiển thị).</summary>
    public string? ParticipantName { get; set; }

    /// <summary>SĐT người tham gia.</summary>
    public string? ParticipantPhone { get; set; }

    public Hl25SpinTurnSource Source { get; set; }
    public int TurnsAdded { get; set; }
    public string? Note { get; set; }
    public DateTime GrantedTime { get; set; }
    public Guid? FrameCreationId { get; set; }
}
