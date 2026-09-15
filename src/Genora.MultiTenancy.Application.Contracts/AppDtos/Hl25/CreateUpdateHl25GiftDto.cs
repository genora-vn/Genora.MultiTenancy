using System.ComponentModel.DataAnnotations;
using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO ghi quà tặng trong kho.</summary>
public class CreateUpdateHl25GiftDto
{
    [Required]
    [StringLength(256)]
    public string Name { get; set; } = null!;

    [StringLength(1024)]
    public string? ImageUrl { get; set; }

    [StringLength(1024)]
    public string? WheelImageUrl { get; set; }

    [StringLength(1000)]
    public string? Description { get; set; }

    [Range(0, int.MaxValue)]
    public int TotalQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int RemainingQuantity { get; set; }

    public decimal? Value { get; set; }

    public Hl25GiftStatus Status { get; set; } = Hl25GiftStatus.Available;
}
