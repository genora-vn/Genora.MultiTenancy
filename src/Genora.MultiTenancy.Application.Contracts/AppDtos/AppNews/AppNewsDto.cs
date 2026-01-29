using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppNews;
public class AppNewsDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public string Title { get; set; }
    public string ShortDescription { get; set; }
    public string ContentHtml { get; set; }
    public string ThumbnailUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    public NewsStatus Status { get; set; }

    public int DisplayOrder { get; set; }

    public List<Guid> RelatedNewsIds { get; set; } = new();
}