using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;

namespace Genora.MultiTenancy.DomainModels.AppDocuments;

[Table("AppDocumentPages")]
public class DocumentPage : FullAuditedAggregateRoot<Guid>
{
    public Guid SectionId { get; set; }
    public virtual DocumentSection? Section { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(200)]
    public string Slug { get; set; } = null!;

    public string ContentHtml { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    /// <summary>
    /// 0: Draft, 1: Published, 2: Hidden
    /// </summary>
    public byte Status { get; set; } = 1;

    [StringLength(200)]
    public string? FeatureName { get; set; }

    [StringLength(200)]
    public string? TenantPermissionName { get; set; }

    [StringLength(200)]
    public string? HostPermissionName { get; set; }

    protected DocumentPage() { }

    public DocumentPage(Guid id, Guid sectionId, string title, string slug) : base(id)
    {
        SectionId = sectionId;
        Title = title;
        Slug = slug;
    }
}
