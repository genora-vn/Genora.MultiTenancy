using AutoMapper;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.Apps.AppSettings;

namespace Genora.MultiTenancy.Web;

public class MultiTenancyWebAutoMapperProfile : Profile
{
    public MultiTenancyWebAutoMapperProfile()
    {

        CreateMap<AppSettingDto, CreateUpdateAppSettingDto>();
        CreateMap<AppCustomerTypeDto, CreateUpdateAppCustomerTypeDto>();

        //Define your object mappings here, for the Web project
    }
}
