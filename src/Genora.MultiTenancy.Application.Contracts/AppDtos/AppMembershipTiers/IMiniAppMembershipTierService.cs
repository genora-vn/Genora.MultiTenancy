using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppMembershipTiers
{
    public interface IMiniAppMembershipTierService : IApplicationService
    {
        Task<PagedResultDto<AppMembershipTierDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    }
}
