using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppLanguages")]
public class AppLanguage : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(20)]
    public string LanguageCode { get; set; } = null!;

    [Required]
    [StringLength(100)]
    public string LanguageName { get; set; } = null!;

    [StringLength(100)]
    public string? NativeName { get; set; }

    public byte Status { get; set; } = 1;

    public int SortOrder { get; set; }
}
