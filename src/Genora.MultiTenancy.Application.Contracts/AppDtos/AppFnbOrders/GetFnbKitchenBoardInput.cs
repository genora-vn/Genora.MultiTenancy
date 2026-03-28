using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class GetFnbKitchenBoardInput
{
    public string? FilterText { get; set; }
    public FnbServiceStatus? ServiceStatus { get; set; }
}