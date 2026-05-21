using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

public interface IMiniAppSalonBeautyTimeSlotAppService : IApplicationService
{
    Task<List<MiniAppSalonBeautyTimeSlotDto>> GetListAsyncTimeSlots(GetMiniAppTimeSlotListInput input);
}
