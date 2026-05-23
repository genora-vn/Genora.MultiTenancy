using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyaltyBonusTiers;

public class SalonBeautyLoyaltyBonusTierDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = null!;
    public decimal MinAmount { get; set; }
    public int BonusPoint { get; set; }
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int DisplayOrder { get; set; }
}

public class CreateSalonBeautyLoyaltyBonusTierDto
{
    [Required]
    [StringLength(100)]
    public string Name { get; set; } = null!;

    [Required]
    [Range(1, 1_000_000_000)]
    public decimal MinAmount { get; set; }

    [Required]
    [Range(0, int.MaxValue)]
    public int BonusPoint { get; set; }

    [StringLength(255)]
    public string? Description { get; set; }

    public bool IsActive { get; set; } = true;

    [Range(0, int.MaxValue)]
    public int DisplayOrder { get; set; }
}

public class UpdateSalonBeautyLoyaltyBonusTierDto : CreateSalonBeautyLoyaltyBonusTierDto { }

public class GetSalonBeautyLoyaltyBonusTierListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public bool? IsActive { get; set; }
}
