using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCustomerTypes
{
    public interface IMiniAppCustomerTypeService : IApplicationService
    {
        Task<AppCustomerTypeDto> GetCustomerTypeByCode(string code);
        Task<PagedResultDto<AppCustomerTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input);
    }
}
