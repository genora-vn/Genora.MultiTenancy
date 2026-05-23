using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyaltyConfigs;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyLoyaltyConfig;

public class IndexModel : MultiTenancyPageModel
{
    public decimal ExchangeRate { get; set; } = 1000m;

    private readonly ISalonBeautyLoyaltyConfigAppService _configService;

    public IndexModel(ISalonBeautyLoyaltyConfigAppService configService)
    {
        _configService = configService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _configService.GetAsync();
        ExchangeRate = dto.ExchangeRate;
    }
}
