using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Sự kiện xếp hạng (chiến dịch). Ranking reset theo sự kiện (BD-5): điểm xếp hạng tính
/// từ các phiên game finish trong khoảng [StartAt, EndAt]. Khớp contract RankingEvent.
/// Schema: HLG.
/// </summary>
[Table("AppHlgRankingEvents", Schema = "HLG")]
public class HlgRankingEvent : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(250)]
    public string Title { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }

    /// <summary>Sự kiện đang kích hoạt (dùng để chọn sự kiện hiện tại cho endpoint ranking/event).</summary>
    public bool IsActive { get; set; } = true;

    protected HlgRankingEvent() { }

    public HlgRankingEvent(Guid id, string title, DateTime startAt, DateTime endAt, Guid? tenantId = null) : base(id)
    {
        Title = title;
        StartAt = startAt;
        EndAt = endAt;
        TenantId = tenantId;
    }
}
