using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyDeposits;

namespace Genora.MultiTenancy.Web.Pages.SalonBeautyDeposits;

public class DetailModalModel : MultiTenancyPageModel
{
    public SalonBeautyDepositDto Deposit { get; set; } = new();

    private readonly ISalonBeautyDepositAppService _depositAppService;

    public DetailModalModel(ISalonBeautyDepositAppService depositAppService)
    {
        _depositAppService = depositAppService;
    }

    public async Task OnGetAsync(Guid id)
    {
        Deposit = await _depositAppService.GetAsync(id);
    }
}
