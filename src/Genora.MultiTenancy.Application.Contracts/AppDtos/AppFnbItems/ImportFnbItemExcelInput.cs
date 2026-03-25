using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public class ImportFnbItemExcelInput
{
    public IRemoteStreamContent? File { get; set; }
}