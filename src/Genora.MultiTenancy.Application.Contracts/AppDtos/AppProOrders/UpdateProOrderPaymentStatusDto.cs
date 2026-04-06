using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class UpdateProOrderPaymentStatusDto
{
    [Required]
    public ProPaymentStatus PaymentStatus { get; set; }
}
