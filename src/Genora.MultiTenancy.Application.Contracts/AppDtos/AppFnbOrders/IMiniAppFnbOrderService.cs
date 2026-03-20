using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public interface IMiniAppFnbOrderService : IApplicationService
{
    Task<MiniAppFnbOrderDetailDto> CreateAsync(CreateFnbOrderDto input);
    Task<MiniAppFnbOrderListDto> GetListAsync(GetMiniAppFnbOrderListInput input);
    Task<MiniAppFnbOrderDetailDto> GetAsync(Guid id);
}