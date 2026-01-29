using System;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppNews;

[Table("AppNewsRelateds")]
public class NewsRelated : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid NewsId { get; set; }
    public Guid RelatedNewsId { get; set; }

    protected NewsRelated() { }

    public NewsRelated(Guid id, Guid newsId, Guid relatedNewsId, Guid? tenantId = null) : base(id)
    {
        NewsId = newsId;
        RelatedNewsId = relatedNewsId;
        TenantId = tenantId;
    }
}