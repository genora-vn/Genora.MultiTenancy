using Volo.Abp.Content;

namespace Genora.MultiTenancy.AppDtos.AppCustomers;
public class ImportCustomerExcelInput
{
    public IRemoteStreamContent? File { get; set; }
}