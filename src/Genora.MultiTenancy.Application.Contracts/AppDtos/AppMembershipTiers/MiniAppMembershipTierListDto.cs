
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppMembershipTiers
{
    public class MiniAppMembershipTierListDto : ZaloBaseResponse
    {
        public PagedResultDto<AppMembershipTierDto> Data { get; set; }
    }
}
