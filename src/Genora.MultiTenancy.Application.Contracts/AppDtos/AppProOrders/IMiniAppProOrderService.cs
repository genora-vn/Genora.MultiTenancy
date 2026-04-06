using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public interface IMiniAppProOrderService : IApplicationService
{
    Task<MiniAppProOrderDetailDto> CreateAsync(CreateProOrderDto input);
    Task<MiniAppProOrderListDto> GetListAsync(GetMiniAppProOrderListInput input);
    Task<MiniAppProOrderDetailDto> GetAsync(Guid id);
    Task<MiniAppProOrderDetailDto> CancelAsync(Guid id, CancelMiniAppProOrderDto input);
}
