using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Lịch sử nhận lượt quay (mỗi lần cộng lượt).
/// LƯU Ý (đã chốt): 1 lượt chỉ cộng khi hoàn tất chu kỳ "Tạo thiệp → Chia sẻ thành công". Tối đa 2 chu kỳ.
/// </summary>
[Table("AppHl25SpinTurnLogs", Schema = "hl25")]
public class Hl25SpinTurnLog : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>FK → Hl25Participant.</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>Nguồn cộng lượt.</summary>
    public Hl25SpinTurnSource Source { get; set; }

    /// <summary>Số lượt cộng (thường +1).</summary>
    public int TurnsAdded { get; set; }

    /// <summary>Ghi chú nguồn (VD id bài chia sẻ).</summary>
    [StringLength(512)]
    public string? Note { get; set; }

    /// <summary>Thời điểm cộng lượt.</summary>
    public DateTime GrantedTime { get; set; }

    /// <summary>FK → Hl25FrameCreation (nếu cộng từ chia sẻ thiệp).</summary>
    public Guid? FrameCreationId { get; set; }

    /// <summary>Navigation tới người tham gia.</summary>
    public virtual Hl25Participant? Participant { get; set; }

    protected Hl25SpinTurnLog() { }

    public Hl25SpinTurnLog(Guid id, Guid participantId, Hl25SpinTurnSource source, int turnsAdded, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        ParticipantId = participantId;
        Source = source;
        TurnsAdded = turnsAdded;
        GrantedTime = DateTime.Now;
    }
}
