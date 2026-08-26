using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO ghi chiến dịch ghép ảnh (không kèm templates — quản lý riêng qua Template AppService).</summary>
public class CreateUpdateHl25FrameCampaignDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(2000)]
    public string? Description { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    public Hl25CampaignStatus Status { get; set; } = Hl25CampaignStatus.Draft;
}
