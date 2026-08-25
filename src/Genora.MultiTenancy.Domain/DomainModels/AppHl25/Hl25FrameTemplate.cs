using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Mẫu frame (khung ảnh) thuộc một chiến dịch ghép ảnh.
/// </summary>
[Table("AppHl25FrameTemplates", Schema = "hl25")]
public class Hl25FrameTemplate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>FK → Hl25FrameCampaign.</summary>
    public Guid CampaignId { get; set; }

    /// <summary>Tên mẫu.</summary>
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    /// <summary>Ảnh mẫu frame (PNG khung trong suốt).</summary>
    [Required]
    [StringLength(1024)]
    public string ImageUrl { get; set; } = null!;

    /// <summary>Ảnh thu nhỏ.</summary>
    [StringLength(1024)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>Thứ tự hiển thị.</summary>
    public int DisplayOrder { get; set; }

    /// <summary>Bật/tắt.</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Navigation tới chiến dịch.</summary>
    public virtual Hl25FrameCampaign? Campaign { get; set; }

    protected Hl25FrameTemplate() { }

    public Hl25FrameTemplate(Guid id, Guid campaignId, string name, string imageUrl, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        CampaignId = campaignId;
        Name = name;
        ImageUrl = imageUrl;
    }
}
