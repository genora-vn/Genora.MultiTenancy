using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class GetProBoardInput
{
    public string? FilterText { get; set; }
    public ProServiceStatus? ServiceStatus { get; set; }
}
