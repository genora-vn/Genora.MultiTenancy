using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppProItems;

public interface IAppProItemService :
    ICrudAppService<
        ProItemDto,
        Guid,
        GetProItemListInput,
        CreateUpdateProItemDto>
{
    Task<ProItemDto> SetStateAsync(Guid id, SetProItemStateDto input);
    Task<IRemoteStreamContent> DownloadImportTemplateAsync();
    Task<IRemoteStreamContent> ExportExcelAsync(GetProItemListInput input);
    Task<int> ImportExcelAsync(ImportProItemExcelInput input);
}
