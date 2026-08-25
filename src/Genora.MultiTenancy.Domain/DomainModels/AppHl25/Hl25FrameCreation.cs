using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Lịch sử tạo ảnh thiệp của người dùng (đối soát & chăm sóc).
/// </summary>
[Table("AppHl25FrameCreations", Schema = "hl25")]
public class Hl25FrameCreation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>FK → Hl25Participant.</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>FK → Hl25FrameCampaign (có thể null nếu chiến dịch đã xóa).</summary>
    public Guid? CampaignId { get; set; }

    /// <summary>FK → Hl25FrameTemplate (có thể null nếu mẫu đã xóa).</summary>
    public Guid? TemplateId { get; set; }

    /// <summary>Ảnh thiệp đã tạo (URL).</summary>
    [Required]
    [StringLength(1024)]
    public string ResultImageUrl { get; set; } = null!;

    /// <summary>Lời chúc (tối đa Hl25Consts.MaxWishLength ký tự).</summary>
    [StringLength(Hl25.Hl25Consts.MaxWishLength)]
    public string? WishMessage { get; set; }

    /// <summary>Link chia sẻ.</summary>
    [StringLength(1024)]
    public string? ShareLink { get; set; }

    /// <summary>Nền tảng chia sẻ.</summary>
    public Hl25SharePlatform SharePlatform { get; set; } = Hl25SharePlatform.None;

    /// <summary>Thời điểm chia sẻ.</summary>
    public DateTime? ShareTime { get; set; }

    /// <summary>Thời điểm tạo ảnh (giữ riêng cho báo cáo).</summary>
    public DateTime CreatedTime { get; set; }

    /// <summary>Navigation tới người tham gia.</summary>
    public virtual Hl25Participant? Participant { get; set; }

    protected Hl25FrameCreation() { }

    public Hl25FrameCreation(Guid id, Guid participantId, string resultImageUrl, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        ParticipantId = participantId;
        ResultImageUrl = resultImageUrl;
        CreatedTime = DateTime.Now;
    }
}
