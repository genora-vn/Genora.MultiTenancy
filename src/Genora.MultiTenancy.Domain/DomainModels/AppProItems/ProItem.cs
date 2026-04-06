using Genora.MultiTenancy.DomainModels.AppProCategories;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppProItems;

[Table("AppProItems")]
public class ProItem : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    public Guid CategoryId { get; set; }
    public virtual ProCategory Category { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsAvailable { get; set; } = true;

    public int SortOrder { get; set; } = 0;

    protected ProItem() { }

    public ProItem(Guid id, Guid categoryId, string name, decimal price, Guid? tenantId = null) : base(id)
    {
        TenantId = tenantId;
        CategoryId = categoryId;
        Name = name;
        Price = price;
    }
}
