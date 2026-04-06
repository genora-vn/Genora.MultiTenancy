using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppProItems;

public class ImportProItemExcelInput
{
    public IRemoteStreamContent? File { get; set; }
}
