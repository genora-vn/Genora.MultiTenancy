using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;

/// <summary>
/// Chỉ áp dụng cho deposit ở Status=Pending.
/// </summary>
public class UpdateSalonBeautyDepositDto
{
    [Range(1000, 1_000_000_000)]
    public decimal Amount { get; set; }

    [Range(1, 3)]
    public byte PaymentMethod { get; set; }

    [StringLength(100)]
    public string? ReferenceCode { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}

public class CancelDepositDto
{
    [Required]
    [StringLength(500)]
    public string CancelReason { get; set; } = null!;
}

public class DepositPreviewResultDto
{
    public decimal ExchangeRate { get; set; }
    public int BasePoint { get; set; }
    public int BonusPoint { get; set; }
    public int TotalPoint { get; set; }
    public System.Guid? BonusTierId { get; set; }
    public string? BonusTierName { get; set; }
}
