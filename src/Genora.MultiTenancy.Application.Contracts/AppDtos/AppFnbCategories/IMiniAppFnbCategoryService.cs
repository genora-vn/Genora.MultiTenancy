using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public interface IMiniAppFnbCategoryService : IApplicationService
{
    Task<MiniAppFnbCategoryListDto> GetListAsync();
}