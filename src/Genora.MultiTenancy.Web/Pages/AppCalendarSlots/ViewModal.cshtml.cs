using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppCalendarSlots
{
    public class ViewModalModel : MultiTenancyPageModel
    {
        [BindProperty(SupportsGet = true)]
        public Guid Id { get; set; }

        public AppCalendarSlotDto Slot { get; set; }

        private readonly IAppCalendarSlotService _slotService;

        public ViewModalModel(IAppCalendarSlotService slotService)
        {
            _slotService = slotService;
        }

        public async Task OnGetAsync()
        {
            Slot = await _slotService.GetAsync(Id);
        }
    }
}
