using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppPromotionTypes
{
    public class CreateModalModel : MultiTenancyPageModel
    {
        [BindProperty]
        public CreateUpdatePromotionTypeDto PromotionType { get; set; }
        private readonly IPromotionTypeService _promotionTypeService;

        public CreateModalModel(IPromotionTypeService promotionTypeService)
        {
            _promotionTypeService = promotionTypeService;
        }

        public void OnGet()
        {
            PromotionType = new CreateUpdatePromotionTypeDto();
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await _promotionTypeService.CreateAsync(PromotionType);
            return NoContent();
        }
    }
}
