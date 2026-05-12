using AutoMapper;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyStylists;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyCustomers;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;

namespace Genora.MultiTenancy.AppServices.SalonBeauty;

public class SalonBeautyApplicationAutoMapperProfile : Profile
{
    public SalonBeautyApplicationAutoMapperProfile()
    {
        CreateMap<SalonBeautyCustomer, SalonBeautyCustomerDto>();
        CreateMap<CreateSalonBeautyCustomerDto, SalonBeautyCustomer>();
        CreateMap<UpdateSalonBeautyCustomerDto, SalonBeautyCustomer>();

        CreateMap<SalonBeautyService, SalonBeautyServiceDto>();
        CreateMap<CreateSalonBeautyServiceDto, SalonBeautyService>();
        CreateMap<UpdateSalonBeautyServiceDto, SalonBeautyService>();

        CreateMap<SalonBeautyServiceCategory, SalonBeautyServiceCategoryDto>();
        CreateMap<CreateSalonBeautyServiceCategoryDto, SalonBeautyServiceCategory>();
        CreateMap<UpdateSalonBeautyServiceCategoryDto, SalonBeautyServiceCategory>();

        CreateMap<SalonBeautyStylist, SalonBeautyStylistDto>();
        CreateMap<CreateSalonBeautyStylistDto, SalonBeautyStylist>();
        CreateMap<UpdateSalonBeautyStylistDto, SalonBeautyStylist>();
    }
}
