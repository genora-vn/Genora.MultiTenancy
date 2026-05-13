using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;

public interface IMiniAppSalonBeautyServiceCategoryAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetListMiniAppAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyServiceCategoryDto> GetMiniAppAsync(Guid id);
}
