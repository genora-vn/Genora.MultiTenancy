using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlPoints;

/// <summary>
/// Lô điểm/tiền khách hàng Hoa Linh đã đổi từ chiến dịch.
/// Mỗi lần đổi tạo 1 lô có hạn +1 năm; dùng cho FIFO tiêu điểm + job hết hạn.
/// </summary>
[Table("AppHlPointBatches", Schema = "HL")]
public class HlPointBatch : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Mã lô (format: PB-{yyMMdd}{seq})</summary>
    [Required]
    [StringLength(50)]
    public string BatchCode { get; set; } = null!;

    // ── Người đổi ──────────────────────────────────────────────────────────
    /// <summary>Id khách hàng trong dbo.AppCustomers (soft reference)</summary>
    public Guid? CustomerId { get; set; }

    [StringLength(50)]
    public string? CustomerCode { get; set; }

    [StringLength(250)]
    public string? CustomerName { get; set; }

    [StringLength(20)]
    public string? CustomerPhone { get; set; }

    // ── Chiến dịch nguồn ───────────────────────────────────────────────────
    [StringLength(50)]
    public string? CampaignCode { get; set; }

    [StringLength(250)]
    public string? CampaignName { get; set; }

    public int? CampaignPeriod { get; set; }

    [StringLength(100)]
    public string? DisplayType { get; set; }

    [StringLength(100)]
    public string? MembershipTier { get; set; }

    // ── Giá trị đổi ────────────────────────────────────────────────────────
    /// <summary>Đơn vị: Point (accumulatedPoints) hoặc Amount (accumulatedSales)</summary>
    public HlPointUnit Unit { get; set; }

    /// <summary>Giá trị gốc lấy từ chiến dịch (accumulatedPoints/accumulatedSales)</summary>
    public decimal SourceValue { get; set; }

    /// <summary>Giá trị cộng vào quỹ (hiện = SourceValue, để chỗ cho tỉ lệ tương lai)</summary>
    public decimal ConvertedValue { get; set; }

    /// <summary>Giá trị còn lại sau khi tiêu (FIFO)</summary>
    public decimal RemainingValue { get; set; }

    public HlPointBatchStatus Status { get; set; } = HlPointBatchStatus.Active;

    // ── Thời hạn ───────────────────────────────────────────────────────────
    public DateTime ExchangedAt { get; set; }

    /// <summary>Hạn dùng = ExchangedAt + 1 năm</summary>
    public DateTime ExpireDate { get; set; }

    protected HlPointBatch() { }

    public HlPointBatch(Guid id, string batchCode, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        BatchCode = batchCode;
    }
}
