using System;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public class DocumentPageContentDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string ContentHtml { get; set; } = string.Empty;
}
