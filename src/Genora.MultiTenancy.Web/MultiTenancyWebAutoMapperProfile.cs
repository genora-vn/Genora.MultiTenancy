using AutoMapper;
using Genora.MultiTenancy.AppDtos.AppBookings;
using Genora.MultiTenancy.AppDtos.AppCalendarSlots;
using Genora.MultiTenancy.AppDtos.AppCustomers;
using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.AppDtos.AppProCategories;
using Genora.MultiTenancy.AppDtos.AppProItems;
using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.AppDtos.AppSpecialDates;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;

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
        CreateMap<SpecialDateDto, CreateUpdateSpecialDateDto>().ReverseMap();

        CreateMap<FeatureGridDto, UpdateFeatureGridDto>()
            .ForMember(d => d.Items, opt => opt.MapFrom(s => s.Items))
            .ReverseMap();
        CreateMap<HomePageWidgetItemDto, UpdateFeatureGridItemDto>().ReverseMap();

        CreateMap<FnbCategoryDto, CreateUpdateFnbCategoryDto>();
        CreateMap<CreateUpdateFnbCategoryDto, FnbCategory>();

        CreateMap<FnbItemDto, CreateUpdateFnbItemDto>();
        CreateMap<CreateUpdateFnbItemDto, FnbItem>();

        // Proshop — cần cho EditModal của ProCategory và ProItem
        CreateMap<ProCategoryDto, CreateUpdateProCategoryDto>();
        CreateMap<ProItemDto, CreateUpdateProItemDto>()
            .ForMember(d => d.Images,        opt => opt.Ignore())
            .ForMember(d => d.IsUploadImage, opt => opt.Ignore());
    }
}
