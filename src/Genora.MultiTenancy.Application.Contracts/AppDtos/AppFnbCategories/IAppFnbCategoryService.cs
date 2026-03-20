using System;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public interface IAppFnbCategoryService :
    ICrudAppService<
        FnbCategoryDto,
        Guid,
        GetFnbCategoryListInput,
        CreateUpdateFnbCategoryDto>
{
}