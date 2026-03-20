using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class GetFnbOrderListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? BagTag { get; set; }
    public FnbServiceStatus? ServiceStatus { get; set; }
    public FnbPaymentStatus? PaymentStatus { get; set; }
    public DateTime? CreationTimeFrom { get; set; }
    public DateTime? CreationTimeTo { get; set; }
}