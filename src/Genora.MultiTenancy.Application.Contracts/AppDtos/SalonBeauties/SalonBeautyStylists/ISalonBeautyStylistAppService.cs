using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;

public interface ISalonBeautyStylistAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyStylistDto>> GetListAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyStylistDto> GetAsync(Guid id);
    Task<SalonBeautyStylistDto> CreateAsync(CreateSalonBeautyStylistDto input);
    Task<SalonBeautyStylistDto> UpdateAsync(Guid id, UpdateSalonBeautyStylistDto input);
    Task DeleteAsync(Guid id);
    Task UpdateShowOnAppAsync(Guid id, bool isShowOnApp);
}
