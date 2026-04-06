using Genora.MultiTenancy.DomainModels.AppProItems;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppProCategories;

[Table("AppProCategories")]
public class ProCategory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(64)]
    public string? Code { get; set; }

    public int SortOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    public virtual ICollection<ProItem> Items { get; set; } = new List<ProItem>();

    protected ProCategory() { }

    public ProCategory(Guid id, string name, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        Name = name;
    }
}
