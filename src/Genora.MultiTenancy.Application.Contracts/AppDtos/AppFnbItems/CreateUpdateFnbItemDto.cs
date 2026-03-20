using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public class CreateUpdateFnbItemDto
{
    [Required]
    public Guid CategoryId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Range(0, double.MaxValue)]
    public decimal Price { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int? SortOrder { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsAvailable { get; set; } = true;
}