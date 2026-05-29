using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public class DocumentTreeNodeDto
{
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public List<DocumentTreePageDto> Pages { get; set; } = new();
}

public class DocumentTreePageDto
{
    public Guid PageId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}

public class DocumentTreeDto
{
    public List<DocumentTreeNodeDto> Sections { get; set; } = new();
}

public class DocumentReadDto
{
    public Guid PageId { get; set; }
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
    public DateTime? LastModificationTime { get; set; }
}
