using AutoMapper;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos.SalonBeautyCustomerDtos;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.SalonBeauty;

namespace Genora.MultiTenancy.Application.SalonBeauty;

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
