using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

/// <summary>
/// Lệnh nạp tiền của khách hàng. Khi tạo có status=Pending.
/// Khi Approve → cộng điểm vào SalonBeautyCustomerLoyaltyBalance + ghi LoyaltyTransaction trong cùng UoW.
/// Đã Success/Cancelled → immutable.
/// </summary>
[Table("AppSalonBeautyDepositTransactions")]
public class SalonBeautyDepositTransaction : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Mã giao dịch sinh tự động: DEP{yyyyMMdd}{seq}.</summary>
    [Required]
    [StringLength(30)]
    public string TransactionCode { get; set; } = null!;

    public Guid CustomerId { get; set; }

    /// <summary>Số tiền nạp (VND).</summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    /// <summary>Tỷ lệ áp dụng tại thời điểm tạo (snapshot, vd: 1000 đ = 1 P → ExchangeRate=1000).</summary>
    [Column(TypeName = "decimal(18,4)")]
    public decimal ExchangeRate { get; set; }

    /// <summary>Số điểm cơ bản = Amount / ExchangeRate (snapshot).</summary>
    public int BasePoint { get; set; }

    /// <summary>Số điểm thưởng theo bonus tier (nếu có).</summary>
    public int BonusPoint { get; set; }

    /// <summary>Tổng điểm thực cộng = BasePoint + BonusPoint (snapshot).</summary>
    public int TotalPoint { get; set; }

    /// <summary>Bonus tier áp dụng (nullable).</summary>
    public Guid? BonusTierId { get; set; }

    /// <summary>Phương thức nạp.</summary>
    public byte PaymentMethod { get; set; }

    [StringLength(100)]
    public string? ReferenceCode { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    /// <summary>Trạng thái: Pending=1, Success=2, Cancelled=3.</summary>
    public byte Status { get; set; } = 1;

    /// <summary>Người duyệt (admin Id, set khi Approve).</summary>
    public Guid? ApprovedBy { get; set; }

    public DateTime? ApprovedAt { get; set; }

    /// <summary>Người hủy.</summary>
    public Guid? CancelledBy { get; set; }

    public DateTime? CancelledAt { get; set; }

    [StringLength(500)]
    public string? CancelReason { get; set; }

    public virtual SalonBeautyCustomer? Customer { get; set; }

    protected SalonBeautyDepositTransaction() { }

    public SalonBeautyDepositTransaction(Guid id, string code, Guid customerId, decimal amount)
        : base(id)
    {
        TransactionCode = code;
        CustomerId = customerId;
        Amount = amount;
    }
}
