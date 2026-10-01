using System;
using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlGiftReceipts;

// Immutable snapshot for reporting. No CRUD/delete endpoint: retries must never reissue a gift.
public class HlGiftReceipt : CreationAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }
    [Required, StringLength(50)] public string ReceiptCode { get; set; } = null!;
    [Required, StringLength(100)] public string CustCode { get; set; } = null!;
    [Required, StringLength(250)] public string CustName { get; set; } = null!;
    [Required, StringLength(20)] public string PhoneNumber { get; set; } = null!;
    [StringLength(1000)] public string? Address { get; set; }
    [Required, StringLength(100)] public string CampaignCode { get; set; } = null!;
    [StringLength(250)] public string? CampaignName { get; set; }
    // DMS null period is stored as 0, so the unique key never contains a nullable period.
    public int CampaignPeriod { get; set; }
    public DateTime? CampaignStartDate { get; set; }
    public DateTime? CampaignEndDate { get; set; }
    [Required, StringLength(100)] public string VoucherCode { get; set; } = null!;
    [Required, StringLength(500)] public string VoucherName { get; set; } = null!;
    public int VoucherType { get; set; } = 2;
    public decimal VoucherValue { get; set; }
    public int Quantity { get; set; }
    public HlGiftReceiptStatus Status { get; set; } = HlGiftReceiptStatus.Confirmed;
    public DateTime ConfirmedAt { get; set; }
    [StringLength(100)] public string? MembershipTier { get; set; }
    public decimal? AccumulatedSales { get; set; }
    public int? AccumulatedPoints { get; set; }
    [StringLength(100)] public string? DsrCode { get; set; }
    [StringLength(250)] public string? DsrName { get; set; }
    [StringLength(100)] public string? DistributorCode { get; set; }
    [StringLength(250)] public string? DistributorName { get; set; }
    [StringLength(30)] public string Source { get; set; } = "ZaloMiniApp";
    [StringLength(1000)] public string? Note { get; set; }

    protected HlGiftReceipt() { }
    public HlGiftReceipt(Guid id, Guid? tenantId) : base(id)
    {
        TenantId = tenantId;
        ReceiptCode = "GR-" + id.ToString("N").ToUpperInvariant();
    }
}
