using AutoMapper;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.Books;

namespace Genora.MultiTenancy.Web;

public class MultiTenancyWebAutoMapperProfile : Profile
{
    public MultiTenancyWebAutoMapperProfile()
    {
        CreateMap<BookDto, CreateUpdateBookDto>();

        CreateMap<AppSettingDto, CreateUpdateAppSettingDto>();

        //Define your object mappings here, for the Web project
    }
}
