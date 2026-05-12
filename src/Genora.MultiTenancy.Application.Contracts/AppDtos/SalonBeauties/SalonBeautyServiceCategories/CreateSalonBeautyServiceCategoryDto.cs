using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;

public class CreateSalonBeautyServiceCategoryDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [StringLength(500)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }

    public byte Status { get; set; } = 1;

    [StringLength(500)]
    public string? Note { get; set; }
}
