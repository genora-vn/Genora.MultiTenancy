using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Mini App gửi lên để yêu cầu tạo order thanh toán
/// </summary>
public class PrepareOrderInput
{
    /// <summary>Id của Booking cần thanh toán</summary>
    public Guid BookingId { get; set; }

    /// <summary>Phương thức thanh toán: COD = 0 | BankTransfer = 2</summary>
    public PaymentMethod PaymentMethod { get; set; }
}
