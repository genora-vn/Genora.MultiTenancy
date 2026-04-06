using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppProItems;

public interface IMiniAppProItemService : IApplicationService
{
    Task<MiniAppProItemListDto> GetListAsync(GetMiniAppProItemListInput input);
    Task<MiniAppProItemDetailDto> GetAsync(Guid id);
}
