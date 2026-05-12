using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;

public class SalonBeautyServiceDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public string? PriceText { get; set; }
    public int Duration { get; set; }
    public string? DurationText { get; set; }
    public byte? ApplicableRole { get; set; }
    public string? ApplicableRoleText { get; set; }
    public byte? ApplicableLevel { get; set; }
    public string? ApplicableLevelText { get; set; }
    public byte Status { get; set; }
    public string? StatusText { get; set; }
    public bool IsShowOnApp { get; set; }
    public string? IsShowOnAppText { get; set; }
    public string? Note { get; set; }
    public int SortOrder { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
}
