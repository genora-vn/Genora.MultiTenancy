using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public class DocumentSectionDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public string? Icon { get; set; }
    public int DisplayOrder { get; set; }
    public string? FeatureName { get; set; }
    public string? TenantPermissionName { get; set; }
    public string? HostPermissionName { get; set; }
    public DocumentStatus Status { get; set; }
    public int PageCount { get; set; }
}
