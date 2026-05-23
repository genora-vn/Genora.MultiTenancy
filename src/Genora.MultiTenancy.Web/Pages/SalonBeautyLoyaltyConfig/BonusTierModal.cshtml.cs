using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyaltyBonusTiers;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyLoyaltyConfig;

public class BonusTierModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public TierForm Tier { get; set; } = new() { IsActive = true, DisplayOrder = 0 };

    public bool IsEdit => Tier.Id.HasValue;

    private readonly ISalonBeautyLoyaltyBonusTierAppService _tierService;

    public BonusTierModalModel(ISalonBeautyLoyaltyBonusTierAppService tierService)
    {
        _tierService = tierService;
    }

    public async Task OnGetAsync(Guid? id)
    {
        if (id.HasValue && id.Value != Guid.Empty)
        {
            var dto = await _tierService.GetAsync(id.Value);
            Tier = new TierForm
            {
                Id = dto.Id,
                Name = dto.Name,
                MinAmount = dto.MinAmount,
                BonusPoint = dto.BonusPoint,
                Description = dto.Description,
                IsActive = dto.IsActive,
                DisplayOrder = dto.DisplayOrder
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid) return Page();

        if (Tier.Id.HasValue && Tier.Id.Value != Guid.Empty)
        {
            await _tierService.UpdateAsync(Tier.Id.Value, new UpdateSalonBeautyLoyaltyBonusTierDto
            {
                Name = Tier.Name!,
                MinAmount = Tier.MinAmount,
                BonusPoint = Tier.BonusPoint,
                Description = Tier.Description,
                IsActive = Tier.IsActive,
                DisplayOrder = Tier.DisplayOrder
            });
        }
        else
        {
            await _tierService.CreateAsync(new CreateSalonBeautyLoyaltyBonusTierDto
            {
                Name = Tier.Name!,
                MinAmount = Tier.MinAmount,
                BonusPoint = Tier.BonusPoint,
                Description = Tier.Description,
                IsActive = Tier.IsActive,
                DisplayOrder = Tier.DisplayOrder
            });
        }

        return NoContent();
    }

    public class TierForm
    {
        public Guid? Id { get; set; }
        public string? Name { get; set; }
        public decimal MinAmount { get; set; }
        public int BonusPoint { get; set; }
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public int DisplayOrder { get; set; }
    }
}
