using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppProOrders;

public class CancelProOrderDto
{
    [Required(ErrorMessage = "Vui lòng chọn lý do hủy đơn.")]
    public ProCancelReason CancelReason { get; set; }

    [StringLength(500)]
    public string? CancelNote { get; set; }
}
