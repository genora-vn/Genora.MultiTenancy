using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyBookings;

public class IndexModel : MultiTenancyPageModel
{
    public List<SelectListItem> LocationItems { get; set; } = new();

    private readonly ISalonBeautyLocationAppService _locationAppService;

    public IndexModel(ISalonBeautyLocationAppService locationAppService)
    {
        _locationAppService = locationAppService;
    }

    public async Task OnGetAsync()
    {
        var locations = await _locationAppService.GetLookupAsync();
        LocationItems = locations
            .Where(x => x.IsActive)
            .Select(x => new SelectListItem(x.Name, x.Id.ToString()))
            .ToList();
    }
}
