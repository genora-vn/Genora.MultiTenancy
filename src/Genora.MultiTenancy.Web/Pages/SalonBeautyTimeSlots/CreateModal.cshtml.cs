using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyTimeSlots;

public class CreateModalModel : MultiTenancyPageModel
{
    public List<SelectListItem> LocationItems { get; set; } = new();
    public List<SalonBeautyLocationLookupDto> Locations { get; set; } = new();

    public Guid? DefaultLocationId { get; set; }
    public DateTime DefaultFromDate { get; set; }
    public DateTime DefaultToDate { get; set; }

    private readonly ISalonBeautyLocationAppService _locationAppService;

    public CreateModalModel(ISalonBeautyLocationAppService locationAppService)
    {
        _locationAppService = locationAppService;
    }

    public async Task OnGetAsync()
    {
        DefaultFromDate = DateTime.Today;
        DefaultToDate = DateTime.Today.AddDays(6);

        Locations = (await _locationAppService.GetLookupAsync()).ToList();

        DefaultLocationId = Locations.FirstOrDefault()?.Id;

        LocationItems = Locations
            .Select(x => new SelectListItem(x.Name, x.Id.ToString(), DefaultLocationId.HasValue && DefaultLocationId.Value == x.Id))
            .ToList();
    }
}
