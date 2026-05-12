using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyStylists;

public class DetailModalModel : MultiTenancyPageModel
{
    public SalonBeautyStylistDto Stylist { get; set; } = new();

    private readonly ISalonBeautyStylistAppService _stylistAppService;

    public DetailModalModel(ISalonBeautyStylistAppService stylistAppService)
    {
        _stylistAppService = stylistAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Stylist = await _stylistAppService.GetAsync(id);
    }
}
