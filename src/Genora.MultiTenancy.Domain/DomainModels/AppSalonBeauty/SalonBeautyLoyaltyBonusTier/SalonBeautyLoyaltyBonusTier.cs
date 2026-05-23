using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

/// <summary>
/// Mốc nạp tiền có tặng thêm điểm.
/// VD: Nạp 500.000 → +50 P bonus, Nạp 2.000.000 → +300 P bonus.
/// Áp dụng theo `MinAmount` cao nhất mà ≤ amount nạp.
/// </summary>
[Table("AppSalonBeautyLoyaltyBonusTiers")]
public class SalonBeautyLoyaltyBonusTier : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    /// <summary>Số tiền tối thiểu (VND) để áp dụng tier này.</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal MinAmount { get; set; }

    /// <summary>Số điểm bonus được tặng thêm.</summary>
    public int BonusPoint { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public int DisplayOrder { get; set; }

    protected SalonBeautyLoyaltyBonusTier() { }

    public SalonBeautyLoyaltyBonusTier(Guid id, string name, decimal minAmount, int bonusPoint)
        : base(id)
    {
        Name = name;
        MinAmount = minAmount;
        BonusPoint = bonusPoint;
    }
}
