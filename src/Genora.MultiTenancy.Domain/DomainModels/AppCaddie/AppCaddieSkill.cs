using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieSkills")]
public class AppCaddieSkill : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string SkillCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string SkillName { get; set; } = null!;

    [StringLength(1000)]
    public string? Description { get; set; }

    public int SortOrder { get; set; }

    public byte Status { get; set; } = 1;
}
