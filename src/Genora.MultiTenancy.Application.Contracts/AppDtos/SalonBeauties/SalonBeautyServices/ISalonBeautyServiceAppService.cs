using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;

public interface ISalonBeautyServiceAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyServiceDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyServiceDto> GetAsync(Guid id);
    Task<SalonBeautyServiceDto> CreateAsync(CreateSalonBeautyServiceDto input);
    Task<SalonBeautyServiceDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceDto input);
    Task DeleteAsync(Guid id);
}
