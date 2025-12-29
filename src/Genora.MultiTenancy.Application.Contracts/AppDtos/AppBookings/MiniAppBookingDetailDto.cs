
using Genora.MultiTenancy.AppDtos.AppZaloAuths;

namespace Genora.MultiTenancy.AppDtos.AppBookings
{
    public class MiniAppBookingDetailDto : ZaloBaseResponse
    {
        public AppBookingDto? Data { get; set; }
    }
}
