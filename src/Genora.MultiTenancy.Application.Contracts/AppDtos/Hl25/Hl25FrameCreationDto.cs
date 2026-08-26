using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO đọc lịch sử tạo ảnh thiệp (read-only).</summary>
public class Hl25FrameCreationDto : AuditedEntityDto<Guid>
{
    public Guid ParticipantId { get; set; }

    /// <summary>Họ tên người tham gia (join sang Participant để hiển thị).</summary>
    public string? ParticipantName { get; set; }

    /// <summary>SĐT người tham gia.</summary>
    public string? ParticipantPhone { get; set; }

    public Guid? CampaignId { get; set; }

    /// <summary>Tên chiến dịch (join sang Campaign để hiển thị).</summary>
    public string? CampaignName { get; set; }

    public Guid? TemplateId { get; set; }

    public string ResultImageUrl { get; set; } = null!;
    public string? WishMessage { get; set; }
    public string? ShareLink { get; set; }
    public Hl25SharePlatform SharePlatform { get; set; }
    public DateTime? ShareTime { get; set; }
    public DateTime CreatedTime { get; set; }
}
