using System;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO đọc quà tặng trong kho.</summary>
public class Hl25GiftDto : AuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public string? ImageUrl { get; set; }
    public string? WheelImageUrl { get; set; }
    public string? Description { get; set; }
    public int TotalQuantity { get; set; }
    public int RemainingQuantity { get; set; }
    public decimal? Value { get; set; }
    public Hl25GiftStatus Status { get; set; }
}
