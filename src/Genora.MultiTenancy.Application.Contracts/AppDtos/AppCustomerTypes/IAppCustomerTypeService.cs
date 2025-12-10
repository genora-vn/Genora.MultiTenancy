using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCustomerTypes;

public interface IAppCustomerTypeService :
        ICrudAppService<
            AppCustomerTypeDto,          // DTO hiển thị
            Guid,                        // Khoá chính
            PagedAndSortedResultRequestDto, // Paging/sorting input
            CreateUpdateAppCustomerTypeDto // DTO create/update
        >
{
}