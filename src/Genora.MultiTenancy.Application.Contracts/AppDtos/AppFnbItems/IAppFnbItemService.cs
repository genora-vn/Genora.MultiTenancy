using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public interface IAppFnbItemService :
    ICrudAppService<
        FnbItemDto,
        Guid,
        GetFnbItemListInput,
        CreateUpdateFnbItemDto>
{
    Task<FnbItemDto> SetStateAsync(Guid id, SetFnbItemStateDto input);
    Task<IRemoteStreamContent> DownloadImportTemplateAsync();
    Task<IRemoteStreamContent> ExportExcelAsync(GetFnbItemListInput input);
    Task<int> ImportExcelAsync(ImportFnbItemExcelInput input);
}