using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Tiến độ học kiến thức per-user. Phục vụ Product.isCompleted (tính per-user),
/// learning-history và stat knowledgeLearned. Schema: HLG.
/// </summary>
[Table("AppHlgLearningProgress", Schema = "HLG")]
public class HlgLearningProgress : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    /// <summary>Liên kết dbo.AppCustomers.</summary>
    public Guid CustomerId { get; set; }

    public Guid ProductId { get; set; }

    /// <summary>Phần trăm hoàn thành 0-100.</summary>
    public int ProgressPercent { get; set; }

    public bool IsCompleted { get; set; }

    public DateTime? CompletedAt { get; set; }

    public DateTime LastViewedAt { get; set; }

    protected HlgLearningProgress() { }

    public HlgLearningProgress(Guid id, Guid customerId, Guid productId, Guid? tenantId = null) : base(id)
    {
        CustomerId = customerId;
        ProductId = productId;
        TenantId = tenantId;
    }
}
