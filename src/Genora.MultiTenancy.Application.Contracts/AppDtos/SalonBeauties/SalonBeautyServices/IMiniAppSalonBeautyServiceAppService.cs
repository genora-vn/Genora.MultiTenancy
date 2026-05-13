using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;

public interface IMiniAppSalonBeautyServiceAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyServiceDto>> GetListMiniAppAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyServiceDto> GetMiniAppAsync(Guid id);
}
