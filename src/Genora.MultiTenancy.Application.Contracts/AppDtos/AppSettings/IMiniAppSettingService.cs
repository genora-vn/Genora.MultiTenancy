using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppSettings
{
    public interface IMiniAppSettingService : IApplicationService
    {
        Task<PagedResultDto<AppSettingDto>> GetListAsync(GetMiniAppSettingListInput input);
        Task<AppSettingDto> GetAsync(Guid id);
    }
}
