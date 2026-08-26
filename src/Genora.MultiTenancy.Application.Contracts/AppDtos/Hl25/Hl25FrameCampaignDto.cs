using System;
using System.Collections.Generic;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO đọc mẫu frame thuộc chiến dịch.</summary>
public class Hl25FrameTemplateDto : AuditedEntityDto<Guid>
{
    public Guid CampaignId { get; set; }
    public string Name { get; set; } = null!;
    public string ImageUrl { get; set; } = null!;
    public string? ThumbnailUrl { get; set; }
    public int DisplayOrder { get; set; }
    public bool IsActive { get; set; }
}

/// <summary>DTO đọc chiến dịch ghép ảnh (kèm danh sách mẫu frame).</summary>
public class Hl25FrameCampaignDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public Hl25CampaignStatus Status { get; set; }
    public int TemplateCount { get; set; }
    public List<Hl25FrameTemplateDto> Templates { get; set; } = new();
}
