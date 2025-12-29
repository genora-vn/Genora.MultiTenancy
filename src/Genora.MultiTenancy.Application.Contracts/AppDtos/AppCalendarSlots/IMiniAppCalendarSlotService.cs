using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public interface IMiniAppCalendarSlotService : IApplicationService
    {
        Task<MiniAppCalendarSlotDto> GetListMiniAppAsync(GetMiniAppCalendarListInput input);
        Task<AppCalendarSlotDto> GetMiniAppAsync(Guid id);
    }
}
