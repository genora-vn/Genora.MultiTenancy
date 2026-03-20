using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class UpdateFnbOrderServiceStatusDto
{
    [Required]
    public FnbServiceStatus ServiceStatus { get; set; }
}