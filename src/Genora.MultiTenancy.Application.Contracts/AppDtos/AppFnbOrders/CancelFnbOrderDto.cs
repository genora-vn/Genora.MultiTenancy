using Genora.MultiTenancy.Enums;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppFnbOrders;
public class CancelFnbOrderDto
{
    [Required(ErrorMessage = "Vui lòng chọn lý do hủy đơn.")]
    public FnbCancelReason CancelReason { get; set; }

    [StringLength(500)]
    public string? CancelNote { get; set; }
}