using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public class ImportFnbCategoryExcelInput
{
    public IRemoteStreamContent? File { get; set; }
}