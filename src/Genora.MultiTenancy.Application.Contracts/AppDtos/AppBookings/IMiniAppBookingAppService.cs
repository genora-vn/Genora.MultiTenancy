using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppBookings;
public interface IMiniAppBookingAppService : IApplicationService
{
    Task<AppBookingDto> CreateFromMiniAppAsync(MiniAppCreateBookingDto input);

    Task<MiniAppBookingListDto> GetListMiniAppAsync(GetMiniAppBookingListInput input);

    Task<MiniAppBookingDetailDto> GetMiniAppAsync(Guid id, Guid customerId);
    Task<MiniAppBookingListDto> GetBookingHistoryAsync(GetMiniAppBookingListInput input);
}