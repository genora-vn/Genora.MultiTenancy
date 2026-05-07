using Volo.Abp.Application.Services;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Volo.Abp.Application.Dtos;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using System;

namespace Genora.MultiTenancy.SalonBeauty;

public interface ISalonBeautyCustomerAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyCustomerDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyCustomerDto> GetAsync(Guid id);
    Task<SalonBeautyCustomerDto> CreateAsync(CreateSalonBeautyCustomerDto input);
    Task<SalonBeautyCustomerDto> UpdateAsync(Guid id, UpdateSalonBeautyCustomerDto input);
    Task DeleteAsync(Guid id);
}
