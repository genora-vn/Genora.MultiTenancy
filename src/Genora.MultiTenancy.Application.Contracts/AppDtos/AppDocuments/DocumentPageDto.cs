using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public class DocumentPageDto : FullAuditedEntityDto<Guid>
{
    public Guid SectionId { get; set; }
    public string SectionName { get; set; } = string.Empty;
    public string SectionSlug { get; set; } = string.Empty;
    public string Title { get; set; } = null!;
    public string Slug { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public DocumentStatus Status { get; set; }
    public string? FeatureName { get; set; }
    public string? TenantPermissionName { get; set; }
    public string? HostPermissionName { get; set; }
}
