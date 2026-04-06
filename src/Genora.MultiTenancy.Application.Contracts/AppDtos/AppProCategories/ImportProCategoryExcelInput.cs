using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppProCategories;

public class ImportProCategoryExcelInput
{
    public IRemoteStreamContent? File { get; set; }
}
