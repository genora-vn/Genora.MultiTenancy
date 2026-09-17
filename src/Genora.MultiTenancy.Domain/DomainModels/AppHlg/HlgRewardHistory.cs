using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Lịch sử đổi quà per-user. Phục vụ contract RewardHistoryItem.
/// pointDelta thường âm (trừ điểm khi đổi). Schema: HLG.
/// </summary>
[Table("AppHlgRewardHistories", Schema = "HLG")]
public class HlgRewardHistory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Người đổi (dbo.AppCustomers).</summary>
    public Guid CustomerId { get; set; }

    public Guid RewardId { get; set; }

    /// <summary>Snapshot tên quà (đề phòng reward bị đổi/xóa sau này).</summary>
    [StringLength(250)]
    public string RewardName { get; set; } = null!;

    /// <summary>Biến động điểm (âm = trừ khi đổi quà).</summary>
    public int PointDelta { get; set; }

    public HlgRewardType RewardType { get; set; }

    public HlgRewardHistoryStatus Status { get; set; } = HlgRewardHistoryStatus.Pending;

    /// <summary>Địa chỉ giao hàng (quà physical của consumer).</summary>
    public Guid? ShippingAddressId { get; set; }

    /// <summary>Mã voucher đã cấp (quà voucher, sau khi phát qua UrBox).</summary>
    [StringLength(100)]
    public string? VoucherCode { get; set; }

    /// <summary>Phiên game phát sinh phần thưởng (nếu đổi ngay sau game).</summary>
    public Guid? SessionId { get; set; }

    protected HlgRewardHistory() { }

    public HlgRewardHistory(Guid id, Guid customerId, Guid rewardId, string rewardName, Guid? tenantId = null) : base(id)
    {
        CustomerId = customerId;
        RewardId = rewardId;
        RewardName = rewardName;
        TenantId = tenantId;
    }
}
