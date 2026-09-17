using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppHlg;

/// <summary>
/// Danh mục kiến thức Gamification. Khớp contract KnowledgeCategory.
/// productCount tính động (không lưu cột). Schema: HLG.
/// </summary>
[Table("AppHlgKnowledgeCategories", Schema = "HLG")]
public class HlgKnowledgeCategory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(250)]
    public string Name { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    [StringLength(1000)]
    public string? ImageUrl { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public virtual ICollection<HlgProduct> Products { get; set; } = new List<HlgProduct>();

    protected HlgKnowledgeCategory() { }

    public HlgKnowledgeCategory(Guid id, string name, Guid? tenantId = null) : base(id)
    {
        Name = name;
        TenantId = tenantId;
    }
}
