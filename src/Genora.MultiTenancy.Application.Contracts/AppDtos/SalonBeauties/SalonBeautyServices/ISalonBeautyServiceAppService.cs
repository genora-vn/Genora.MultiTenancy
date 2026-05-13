using System;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;

public interface ISalonBeautyServiceAppService :
    ICrudAppService<
        SalonBeautyServiceDto,
        Guid,
        GetSalonBeautyListInput,
        CreateSalonBeautyServiceDto,
        UpdateSalonBeautyServiceDto>
{
}
