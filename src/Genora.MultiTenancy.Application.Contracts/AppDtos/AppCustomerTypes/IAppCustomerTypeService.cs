using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCustomerTypes;

public interface IAppCustomerTypeService :
    ICrudAppService<
        AppCustomerTypeDto,
        Guid,
        PagedAndSortedResultRequestDto,
        CreateUpdateAppCustomerTypeDto,
        CreateUpdateAppCustomerTypeDto
    >
{
    // Dùng riêng cho DataTable search (serverSide)
    Task<PagedResultDto<AppCustomerTypeDto>> GetListWithFilterAsync(GetCustomerTypeInput input);
}