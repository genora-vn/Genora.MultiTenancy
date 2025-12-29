using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppSettings
{
    public class MiniAppAppSettingListDto : ZaloBaseResponse
    {
        public PagedResultDto<AppSettingDto> Data { get; set; }
    }
}
