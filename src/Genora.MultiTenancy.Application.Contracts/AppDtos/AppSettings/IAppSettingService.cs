using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppSettings;

public interface IAppSettingService :
    ICrudAppService<
        AppSettingDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAppSettingDto>
{
    Task<PagedResultDto<AppSettingDto>> GetListWithFilterAsync(GetMiniAppSettingListInput input);
}