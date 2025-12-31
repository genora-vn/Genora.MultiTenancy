using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppNews;

[Table("AppNews")]
public class News : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid? GolfCourseId { get; set; }
    public virtual GolfCourse? GolfCourse { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;
    [Required]
    [StringLength(1000)]
    public string ShortDescription { get; set; } = null!;

    public string ContentHtml { get; set; } = null!;

    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    /// <summary>
    /// 0: Draft, 1: Visible, 2: Hidden
    /// </summary>
    public byte Status { get; set; } = 0;

    public int DisplayOrder { get; set; }

    protected News() { }

    public News(Guid id, string title, string contentHtml) : base(id)
    {
        Title = title;
        ContentHtml = contentHtml;
    }
}
