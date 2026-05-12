using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;

public class CreateSalonBeautyServiceDto
{
    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    public Guid CategoryId { get; set; }

    [Range(typeof(decimal), "0", "9999999999999999")]
    public decimal Price { get; set; }

    [Range(1, 1440)]
    public int Duration { get; set; }

    [Required]
    public byte? ApplicableRole { get; set; }

    [Required]
    public byte? ApplicableLevel { get; set; }

    public byte Status { get; set; } = 1;

    public bool IsShowOnApp { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [Range(0, int.MaxValue)]
    public int SortOrder { get; set; }
}
