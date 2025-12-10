using AutoMapper;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.Apps.AppSettings;

namespace Genora.MultiTenancy.Web;

public class MultiTenancyWebAutoMapperProfile : Profile
{
    public MultiTenancyWebAutoMapperProfile()
    {

        CreateMap<AppSettingDto, CreateUpdateAppSettingDto>();
        CreateMap<AppCustomerTypeDto, CreateUpdateAppCustomerTypeDto>();
        CreateMap<AppMembershipTierDto, CreateUpdateAppMembershipTierDto>();
        //Define your object mappings here, for the Web project
    }
}
