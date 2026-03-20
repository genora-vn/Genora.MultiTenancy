using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public class CreateUpdateFnbCategoryDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(64)]
    public string? Code { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    public bool IsActive { get; set; } = true;
}