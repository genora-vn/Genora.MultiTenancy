using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppBookings
{
    public class MiniAppBookingListDto : ZaloBaseResponse
    {
        public PagedResultDto<AppBookingDto>? Data { get; set; }
    }
}
