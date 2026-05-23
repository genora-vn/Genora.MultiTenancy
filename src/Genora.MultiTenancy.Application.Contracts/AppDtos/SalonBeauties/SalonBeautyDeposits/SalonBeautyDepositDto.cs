using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;

public class SalonBeautyDepositDto : FullAuditedEntityDto<Guid>
{
    public string TransactionCode { get; set; } = null!;

    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerCode { get; set; }
    public string? CustomerPhone { get; set; }

    public decimal Amount { get; set; }

    public decimal ExchangeRate { get; set; }

    public int BasePoint { get; set; }
    public int BonusPoint { get; set; }
    public int TotalPoint { get; set; }

    public Guid? BonusTierId { get; set; }
    public string? BonusTierName { get; set; }

    public byte PaymentMethod { get; set; }
    public string? PaymentMethodText { get; set; }

    public string? ReferenceCode { get; set; }
    public string? Note { get; set; }

    public byte Status { get; set; }
    public string? StatusText { get; set; }

    public Guid? ApprovedBy { get; set; }
    public DateTime? ApprovedAt { get; set; }

    public Guid? CancelledBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public string? CancelReason { get; set; }
}
