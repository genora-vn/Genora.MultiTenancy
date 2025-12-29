using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppSettings
{
    public interface IMiniAppSettingService : IApplicationService
    {
        Task<MiniAppAppSettingListDto> GetListAsync(GetMiniAppSettingListInput input);
        Task<MiniAppAppSettingDetailDto> GetAsync(Guid id);
    }
}
