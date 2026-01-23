using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppMembershipTiers;

public interface IAppMembershipTierService :
    ICrudAppService<
        AppMembershipTierDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAppMembershipTierDto>
{
    Task<PagedResultDto<AppMembershipTierDto>> GetListWithFilterAsync(GetMiniAppAppMembershipTierListInput input);
}
