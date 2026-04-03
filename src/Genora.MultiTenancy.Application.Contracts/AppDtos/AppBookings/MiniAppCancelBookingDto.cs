using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

/// <summary>
/// Input để huỷ booking từ Mini App.
/// CustomerId dùng để xác thực chủ booking — chỉ chủ mới được huỷ.
/// </summary>
public class MiniAppCancelBookingDto
{
    /// <summary>Id của Customer đang đăng nhập Mini App (để xác thực quyền huỷ)</summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>Lý do huỷ (optional — lưu vào InternalNote để admin tra cứu)</summary>
    [StringLength(500)]
    public string? CancelReason { get; set; }
}
