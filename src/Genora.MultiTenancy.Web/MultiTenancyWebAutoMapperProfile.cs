using AutoMapper;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;

namespace Genora.MultiTenancy.Web;

public class MultiTenancyWebAutoMapperProfile : Profile
{
    public MultiTenancyWebAutoMapperProfile()
    {

        CreateMap<AppSettingDto, CreateUpdateAppSettingDto>();
        CreateMap<AppCustomerTypeDto, CreateUpdateAppCustomerTypeDto>();
        CreateMap<AppGolfCourseDto, CreateUpdateAppGolfCourseDto>();
        CreateMap<AppMembershipTierDto, CreateUpdateAppMembershipTierDto>();
        //Define your object mappings here, for the Web project
    }
}
