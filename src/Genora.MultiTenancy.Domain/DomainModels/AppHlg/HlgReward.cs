using Genora.MultiTenancy.Enums.Hlg;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Phần thưởng có thể đổi bằng điểm. Khớp contract Reward. type: physical | voucher.
/// Schema: HLG.
/// </summary>
[Table("AppHlgRewards", Schema = "HLG")]
public class HlgReward : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    /// <summary>Số điểm cần để đổi (contract: pointCost).</summary>
    public int PointCost { get; set; }

    public HlgRewardType Type { get; set; }

    /// <summary>Số lượng còn lại (null = không giới hạn).</summary>
    public int? StockQuantity { get; set; }

    /// <summary>Mã voucher UrBox (chỉ với type=voucher).</summary>
    [StringLength(100)]
    public string? VoucherCode { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    protected HlgReward() { }

    public HlgReward(Guid id, string name, HlgRewardType type, int pointCost, Guid? tenantId = null) : base(id)
    {
        Name = name;
        Type = type;
        PointCost = pointCost;
        TenantId = tenantId;
    }
}
