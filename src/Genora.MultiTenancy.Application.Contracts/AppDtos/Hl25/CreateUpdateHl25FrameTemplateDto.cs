using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO ghi mẫu frame thuộc chiến dịch.</summary>
public class CreateUpdateHl25FrameTemplateDto
{
    [Required]
    public Guid CampaignId { get; set; }

    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(1024)]
    public string ImageUrl { get; set; } = null!;

    [StringLength(1024)]
    public string? ThumbnailUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;
}
