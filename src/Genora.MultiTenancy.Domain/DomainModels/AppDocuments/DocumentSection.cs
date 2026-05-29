using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppDocuments;

[Table("AppDocumentSections")]
public class DocumentSection : FullAuditedAggregateRoot<Guid>
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Slug { get; set; } = null!;

    [StringLength(100)]
    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    [StringLength(200)]
    public string? FeatureName { get; set; }

    [StringLength(200)]
    public string? TenantPermissionName { get; set; }

    [StringLength(200)]
    public string? HostPermissionName { get; set; }

    /// <summary>
    /// 0: Draft, 1: Published, 2: Hidden
    /// </summary>
    public byte Status { get; set; } = 1;

    public virtual ICollection<DocumentPage> Pages { get; set; } = new List<DocumentPage>();

    protected DocumentSection() { }

    public DocumentSection(Guid id, string name, string slug) : base(id)
    {
        Name = name;
        Slug = slug;
    }
}
