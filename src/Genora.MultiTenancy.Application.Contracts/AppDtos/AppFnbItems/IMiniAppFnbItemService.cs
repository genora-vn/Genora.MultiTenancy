using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public interface IMiniAppFnbItemService : IApplicationService
{
    Task<MiniAppFnbItemListDto> GetListAsync(GetMiniAppFnbItemListInput input);
    Task<MiniAppFnbItemDetailDto> GetAsync(Guid id);
}