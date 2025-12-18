using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public interface IMiniAppCalendarSlotService : IApplicationService
    {
        Task<PagedResultDto<AppCalendarSlotDto>> GetListMiniAppAsync(GetCalendarSlotListInput input);
        Task<AppCalendarSlotDto> GetMiniAppAsync(Guid id);
    }
}
