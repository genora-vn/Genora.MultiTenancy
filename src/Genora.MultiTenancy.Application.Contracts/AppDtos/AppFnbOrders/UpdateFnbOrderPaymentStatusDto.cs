using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class UpdateFnbOrderPaymentStatusDto
{
    [Required]
    public FnbPaymentStatus PaymentStatus { get; set; }
}