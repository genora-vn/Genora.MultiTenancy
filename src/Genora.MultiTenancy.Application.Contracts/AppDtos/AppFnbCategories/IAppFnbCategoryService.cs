using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public interface IAppFnbCategoryService :
    ICrudAppService<
        FnbCategoryDto,
        Guid,
        GetFnbCategoryListInput,
        CreateUpdateFnbCategoryDto>
{
    Task<FnbCategoryDto> SetActiveAsync(Guid id, SetFnbCategoryActiveDto input);
    Task<IRemoteStreamContent> DownloadImportTemplateAsync();
    Task<IRemoteStreamContent> ExportExcelAsync(GetFnbCategoryListInput input);
    Task<int> ImportExcelAsync(ImportFnbCategoryExcelInput input);
}