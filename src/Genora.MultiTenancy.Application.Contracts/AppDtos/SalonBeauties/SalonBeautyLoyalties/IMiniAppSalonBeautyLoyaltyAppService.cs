using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyLoyalties;

public interface IMiniAppSalonBeautyLoyaltyAppService : IApplicationService
{
    Task<CustomerLoyaltyBalanceDto> GetBalanceMiniAppAsync(Guid customerId);
}