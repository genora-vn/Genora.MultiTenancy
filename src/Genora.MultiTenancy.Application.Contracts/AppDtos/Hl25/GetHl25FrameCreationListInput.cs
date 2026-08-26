using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input lọc lịch sử tạo ảnh thiệp.</summary>
public class GetHl25FrameCreationListInput : PagedAndSortedResultRequestDto
{
    /// <summary>Lọc theo chiến dịch.</summary>
    public Guid? CampaignId { get; set; }

    /// <summary>Lọc theo người tham gia.</summary>
    public Guid? ParticipantId { get; set; }

    /// <summary>Lọc theo nền tảng chia sẻ.</summary>
    public Hl25SharePlatform? SharePlatform { get; set; }

    /// <summary>Tìm theo tên/SĐT người tham gia.</summary>
    public string? FilterText { get; set; }

    public DateTime? CreatedFrom { get; set; }
    public DateTime? CreatedTo { get; set; }
}
