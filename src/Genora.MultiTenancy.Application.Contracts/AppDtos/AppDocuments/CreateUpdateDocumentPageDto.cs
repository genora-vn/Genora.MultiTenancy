using Genora.MultiTenancy.Enums;
using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public class CreateUpdateDocumentPageDto
{
    [Required]
    public Guid SectionId { get; set; }

    [Required]
    [StringLength(255)]
    public string Title { get; set; } = null!;

    [StringLength(200)]
    [RegularExpression(@"^[a-z0-9]+(-[a-z0-9]+)*$",
        ErrorMessage = "Slug chỉ chứa chữ thường, số và dấu gạch ngang.")]
    public string? Slug { get; set; }

    [Required]
    public string ContentHtml { get; set; } = string.Empty;

    public int DisplayOrder { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Published;

    [StringLength(200)]
    public string? FeatureName { get; set; }

    [StringLength(200)]
    public string? TenantPermissionName { get; set; }

    [StringLength(200)]
    public string? HostPermissionName { get; set; }
}
