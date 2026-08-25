using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHl25;

/// <summary>
/// Lịch sử lượt quay đã thực hiện.
/// LƯU Ý (đã chốt): trao thưởng 2 bước — quay trúng = Won, Admin xác nhận → Delivered.
/// </summary>
[Table("AppHl25SpinLogs", Schema = "hl25")]
public class Hl25SpinLog : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>FK → Hl25Participant.</summary>
    public Guid ParticipantId { get; set; }

    /// <summary>FK → Hl25WheelSlot (ô trúng).</summary>
    public Guid? WheelSlotId { get; set; }

    /// <summary>FK → Hl25Gift (quà trúng, null nếu trượt).</summary>
    public Guid? GiftId { get; set; }

    /// <summary>Snapshot tên quà tại thời điểm quay.</summary>
    [StringLength(256)]
    public string? GiftNameSnapshot { get; set; }

    /// <summary>Thời điểm quay.</summary>
    public DateTime SpinTime { get; set; }

    /// <summary>Trạng thái trao thưởng.</summary>
    public Hl25RewardStatus RewardStatus { get; set; } = Hl25RewardStatus.Pending;

    /// <summary>Thời điểm trao thưởng.</summary>
    public DateTime? DeliveredTime { get; set; }

    /// <summary>Snapshot địa chỉ nhận quà tại thời điểm quay.</summary>
    [StringLength(512)]
    public string? ReceiverAddressSnapshot { get; set; }

    /// <summary>Navigation tới người tham gia.</summary>
    public virtual Hl25Participant? Participant { get; set; }

    protected Hl25SpinLog() { }

    public Hl25SpinLog(Guid id, Guid participantId, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        ParticipantId = participantId;
        SpinTime = DateTime.Now;
    }
}
