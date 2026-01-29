using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.AppDtos.AppNews;
public class MiniAppRelatedNewsData
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string? ShortDescription { get; set; }
    public string? ContentHtml { get; set; }
    public string? ThumbnailUrl { get; set; }
    public DateTime? PublishedAt { get; set; }
    public NewsStatus Status { get; set; }
    public int DisplayOrder { get; set; }

}