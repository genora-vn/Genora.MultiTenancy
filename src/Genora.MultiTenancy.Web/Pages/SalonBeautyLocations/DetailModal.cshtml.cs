using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLocations;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyLocations;

public class DetailModalModel : MultiTenancyPageModel
{
    public SalonBeautyLocationDto Location { get; set; } = new();

    private readonly ISalonBeautyLocationAppService _locationAppService;

    public DetailModalModel(ISalonBeautyLocationAppService locationAppService)
    {
        _locationAppService = locationAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Location = await _locationAppService.GetAsync(id);
    }
}
