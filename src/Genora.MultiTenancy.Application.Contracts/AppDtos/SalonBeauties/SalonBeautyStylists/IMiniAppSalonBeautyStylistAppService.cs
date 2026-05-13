using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;

public interface IMiniAppSalonBeautyStylistAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyStylistDto>> GetListMiniAppAsync(GetSalonBeautyListInput input);
    Task<SalonBeautyStylistDto> GetMiniAppAsync(Guid id);
}