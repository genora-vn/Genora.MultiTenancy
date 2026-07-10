using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlPoints;

/// <summary>
/// Sổ cái biến động điểm thưởng Hoa Linh — ghi mọi lần cộng (đổi) / trừ (tiêu, hết hạn).
/// Dùng cho trang quản trị "Lịch sử điểm thưởng".
/// </summary>
[Table("AppHlPointTransactions", Schema = "HL")]
public class HlPointTransaction : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    // ── Khách hàng ─────────────────────────────────────────────────────────
    public Guid? CustomerId { get; set; }

    [StringLength(50)]
    public string? CustomerCode { get; set; }

    [StringLength(250)]
    public string? CustomerName { get; set; }

    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    // ── Giao dịch ──────────────────────────────────────────────────────────
    public HlPointTransactionType Type { get; set; }

    public HlPointUnit Unit { get; set; }

    /// <summary>Giá trị biến động: dương = cộng, âm = trừ</summary>
    public decimal Value { get; set; }

    /// <summary>Số dư điểm (BonusPoint) sau giao dịch</summary>
    public decimal BalancePointAfter { get; set; }

    /// <summary>Số dư tiền (BonusAmount) sau giao dịch</summary>
    public decimal BalanceAmountAfter { get; set; }

    // ── Tham chiếu ─────────────────────────────────────────────────────────
    /// <summary>Lô điểm liên quan (nếu có)</summary>
    public Guid? BatchId { get; set; }

    /// <summary>Mã tham chiếu: BatchCode (đổi) / ExchangeCode (đổi quà)</summary>
    [StringLength(50)]
    public string? RefCode { get; set; }

    [StringLength(500)]
    public string? Description { get; set; }

    protected HlPointTransaction() { }

    public HlPointTransaction(Guid id, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
    }
}
