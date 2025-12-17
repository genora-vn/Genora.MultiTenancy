using AutoMapper;
using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.ZaloAuths;

namespace Genora.MultiTenancy.Web;

public class MultiTenancyWebAutoMapperProfile : Profile
{
    public MultiTenancyWebAutoMapperProfile()
    {

        CreateMap<AppSettingDto, CreateUpdateAppSettingDto>();
        CreateMap<AppCustomerTypeDto, CreateUpdateAppCustomerTypeDto>();
        CreateMap<AppGolfCourseDto, CreateUpdateAppGolfCourseDto>();
        CreateMap<AppMembershipTierDto, CreateUpdateAppMembershipTierDto>();
        CreateMap<AppCustomerDto, CreateUpdateAppCustomerDto>();
        CreateMap<AppCalendarSlotDto, CreateUpdateAppCalendarSlotDto>();
        CreateMap<AppNewsDto, CreateUpdateAppNewsDto>();
        CreateMap<AppBookingDto, CreateUpdateAppBookingDto>();

        CreateMap<AppZaloAuthDto, CreateUpdateZaloAuthDto>();
        //Define your object mappings here, for the Web project
    }
}
