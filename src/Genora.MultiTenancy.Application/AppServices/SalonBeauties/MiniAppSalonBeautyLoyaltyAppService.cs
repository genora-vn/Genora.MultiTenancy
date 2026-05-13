using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.MiniApps;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.SalonBeauties.MiniApps;

public class MiniAppSalonBeautyLoyaltyAppService : ApplicationService, IMiniAppSalonBeautyLoyaltyAppService
{
    private readonly IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> _balanceRepository;

    public MiniAppSalonBeautyLoyaltyAppService(IRepository<SalonBeautyCustomerLoyaltyBalance, Guid> balanceRepository)
    {
        _balanceRepository = balanceRepository;
    }

    public async Task<CustomerLoyaltyBalanceDto> GetBalanceMiniAppAsync(Guid customerId)
    {
        var balance = await _balanceRepository.FirstOrDefaultAsync(x => x.CustomerId == customerId);
        return new CustomerLoyaltyBalanceDto
        {
            Id = balance?.Id ?? Guid.Empty,
            CustomerId = customerId,
            CurrentPoint = balance?.CurrentPoint ?? 0
        };
    }
}
