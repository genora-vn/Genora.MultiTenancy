using Genora.MultiTenancy.AppDtos.AppNews;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppPromotionTypes
{
    public interface IPromotionTypeService : ICrudAppService<
            AppPromotionTypeDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdatePromotionTypeDto>
    {
    }
}
