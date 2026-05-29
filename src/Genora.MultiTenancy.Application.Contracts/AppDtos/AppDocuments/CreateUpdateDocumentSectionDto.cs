using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppDocuments;

public class CreateUpdateDocumentSectionDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(200)]
    [RegularExpression(@"^[a-z0-9]+(-[a-z0-9]+)*$",
        ErrorMessage = "Slug chỉ chứa chữ thường, số và dấu gạch ngang.")]
    public string? Slug { get; set; }

    [StringLength(100)]
    public string? Icon { get; set; }

    public int DisplayOrder { get; set; }

    [StringLength(200)]
    public string? FeatureName { get; set; }

    [StringLength(200)]
    public string? TenantPermissionName { get; set; }

    [StringLength(200)]
    public string? HostPermissionName { get; set; }

    public DocumentStatus Status { get; set; } = DocumentStatus.Published;
}
