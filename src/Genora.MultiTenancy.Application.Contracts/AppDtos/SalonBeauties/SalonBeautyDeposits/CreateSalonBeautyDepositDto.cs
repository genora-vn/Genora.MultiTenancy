using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;

public class CreateSalonBeautyDepositDto
{
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>Số tiền nạp (VND).</summary>
    [Required]
    [Range(1000, 1_000_000_000)]
    public decimal Amount { get; set; }

    /// <summary>Cash=1, BankTransfer=2, EWallet=3.</summary>
    [Required]
    [Range(1, 3)]
    public byte PaymentMethod { get; set; } = 1;

    [StringLength(100)]
    public string? ReferenceCode { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }
}
