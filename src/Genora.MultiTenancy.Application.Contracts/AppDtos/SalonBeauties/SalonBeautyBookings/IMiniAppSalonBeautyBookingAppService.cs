using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;

public interface IMiniAppSalonBeautyBookingAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyBookingDetailDto>> GetListMiniAppAsync(GetSalonBeautyBookingListInput input);
    Task<SalonBeautyBookingDetailDto> GetMiniAppAsync(Guid id);
    Task<SalonBeautyBookingDetailDto> CreateMiniAppAsync(CreateSalonBeautyBookingDto input);
    Task<SalonBeautyBookingDetailDto> CancelMiniAppAsync(Guid id, CancelBookingDto input);
}
