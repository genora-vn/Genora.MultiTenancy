using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class UpdateProOrderServiceStatusDto
{
    [Required]
    public ProServiceStatus ServiceStatus { get; set; }

    public string? InternalNote { get; set; }
}
