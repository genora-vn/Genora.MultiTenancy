using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class GetProOrderListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public string? BagTag { get; set; }
    public ProServiceStatus? ServiceStatus { get; set; }
    public ProPaymentStatus? PaymentStatus { get; set; }
    public DateTime? CreationTimeFrom { get; set; }
    public DateTime? CreationTimeTo { get; set; }
}
