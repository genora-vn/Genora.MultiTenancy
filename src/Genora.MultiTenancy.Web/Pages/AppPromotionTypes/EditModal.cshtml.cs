using Genora.MultiTenancy.AppDtos.AppPromotionTypes;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppPromotionTypes
{
    public class EditModalModel : MultiTenancyPageModel
    {
        [HiddenInput]
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        [BindProperty]
        public CreateUpdatePromotionTypeDto PromotionType { get; set; }
        private readonly IPromotionTypeService _promotionTypeService;

        public EditModalModel(IPromotionTypeService promotionTypeService)
        {
            _promotionTypeService = promotionTypeService;
        }

        public async Task OnGetAsync()
        {
            var appPromotionTypeDto = await _promotionTypeService.GetAsync(Id);
            PromotionType = ObjectMapper.Map<AppPromotionTypeDto, CreateUpdatePromotionTypeDto>(appPromotionTypeDto);
        }
        public async Task<IActionResult> OnPostAsync()
        {
            await _promotionTypeService.UpdateAsync(Id, PromotionType);
            return NoContent();
        }
    }
}
