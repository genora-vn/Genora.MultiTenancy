using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public class GetDocumentSectionListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public DocumentStatus? Status { get; set; }
}

public class GetDocumentPageListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? SectionId { get; set; }
    public DocumentStatus? Status { get; set; }
}
