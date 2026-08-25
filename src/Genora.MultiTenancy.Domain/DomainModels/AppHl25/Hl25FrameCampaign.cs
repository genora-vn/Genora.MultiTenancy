using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Chiến dịch/đợt ghép ảnh (Frame) của chương trình 25 năm.
/// </summary>
[Table("AppHl25FrameCampaigns", Schema = "hl25")]
public class Hl25FrameCampaign : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Tên chiến dịch.</summary>
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    /// <summary>Mô tả.</summary>
    [StringLength(2000)]
    public string? Description { get; set; }

    /// <summary>Thời gian bắt đầu.</summary>
    public DateTime? StartTime { get; set; }

    /// <summary>Thời gian kết thúc.</summary>
    public DateTime? EndTime { get; set; }

    /// <summary>Trạng thái chiến dịch.</summary>
    public Hl25CampaignStatus Status { get; set; } = Hl25CampaignStatus.Draft;

    /// <summary>Danh sách mẫu frame thuộc chiến dịch (dùng WithDetailsAsync khi cần).</summary>
    public virtual ICollection<Hl25FrameTemplate> Templates { get; set; } = new List<Hl25FrameTemplate>();

    protected Hl25FrameCampaign() { }

    public Hl25FrameCampaign(Guid id, string name, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        Name = name;
    }
}
