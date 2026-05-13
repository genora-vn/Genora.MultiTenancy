using System;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;

public interface ISalonBeautyServiceCategoryAppService :
    ICrudAppService<
        SalonBeautyServiceCategoryDto,
        Guid,
        GetSalonBeautyListInput,
        CreateSalonBeautyServiceCategoryDto,
        UpdateSalonBeautyServiceCategoryDto>
{
}
