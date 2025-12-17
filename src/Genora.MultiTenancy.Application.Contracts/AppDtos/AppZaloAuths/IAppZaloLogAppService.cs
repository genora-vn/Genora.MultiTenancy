using Genora.MultiTenancy.AppDtos.ZaloAuths;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppZaloAuths;
public interface IAppZaloLogAppService : IApplicationService
{
    Task<PagedResultDto<AppZaloLogDto>> GetListAsync(GetZaloLogListInput input);
    Task<AppZaloLogDto> GetAsync(Guid id);
}