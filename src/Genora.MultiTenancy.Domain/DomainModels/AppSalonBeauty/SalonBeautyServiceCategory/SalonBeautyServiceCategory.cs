using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyServiceCategories")]
public class SalonBeautyServiceCategory : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public byte Status { get; set; } = 1;

    [StringLength(500)]
    public string? Note { get; set; }

    public virtual ICollection<SalonBeautyService> Services { get; set; } = new List<SalonBeautyService>();
}
