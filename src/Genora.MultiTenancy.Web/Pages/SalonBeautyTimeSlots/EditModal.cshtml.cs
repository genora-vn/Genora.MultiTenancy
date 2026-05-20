using System;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyTimeSlots;

public class EditModalModel : MultiTenancyPageModel
{
    public Guid StylistId { get; set; }

    public void OnGet(Guid stylistId)
    {
        StylistId = stylistId;
    }
}
