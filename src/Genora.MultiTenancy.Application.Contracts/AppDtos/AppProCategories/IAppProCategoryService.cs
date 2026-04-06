using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppProCategories;

public interface IAppProCategoryService :
    ICrudAppService<
        ProCategoryDto,
        Guid,
        GetProCategoryListInput,
        CreateUpdateProCategoryDto>
{
    Task<ProCategoryDto> SetActiveAsync(Guid id, SetProCategoryActiveDto input);
    Task<IRemoteStreamContent> DownloadImportTemplateAsync();
    Task<IRemoteStreamContent> ExportExcelAsync(GetProCategoryListInput input);
    Task<int> ImportExcelAsync(ImportProCategoryExcelInput input);
}
