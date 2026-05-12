using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;

public interface ISalonBeautyServiceCategoryAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyServiceCategoryDto> GetAsync(Guid id);
    Task<SalonBeautyServiceCategoryDto> CreateAsync(CreateSalonBeautyServiceCategoryDto input);
    Task<SalonBeautyServiceCategoryDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceCategoryDto input);
    Task DeleteAsync(Guid id);
}