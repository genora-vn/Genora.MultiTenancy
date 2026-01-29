using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppNews;

public class CreateUpdateAppNewsDto
{
    [Required]
    [StringLength(200)]
    public string Title { get; set; }

    [Required]
    [StringLength(1000)]
    public string ShortDescription { get; set; }

    [Required]
    public string ContentHtml { get; set; }

    [StringLength(500)]
    public string? ThumbnailUrl { get; set; }

    public DateTime? PublishedAt { get; set; }

    [Required]
    public NewsStatus Status { get; set; }

    public int DisplayOrder { get; set; } = 0;

    public bool IsActive { get; set; } = true;

    // Single file
    public IRemoteStreamContent? Images { get; set; }

    public bool IsUploadImage { get; set; }

    public List<Guid> RelatedNewsIds { get; set; } = new();
}
