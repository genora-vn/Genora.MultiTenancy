using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppBookings;
public interface IMiniAppBookingAppService : IApplicationService
{
    Task<AppBookingDto> CreateFromMiniAppAsync(MiniAppCreateBookingDto input);

    Task<PagedResultDto<AppBookingDto>> GetListMiniAppAsync(GetMiniAppBookingListInput input);

    Task<AppBookingDto> GetMiniAppAsync(Guid id, Guid customerId);
    Task<MiniAppBookingListDto> GetBookingHistoryAsync(GetMiniAppBookingListInput input);
}