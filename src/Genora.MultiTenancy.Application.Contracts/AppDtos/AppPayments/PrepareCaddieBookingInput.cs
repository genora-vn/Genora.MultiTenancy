using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Mini App gửi lên để yêu cầu tạo order thanh toán cho đặt Caddie.
/// </summary>
public class PrepareCaddieBookingInput
{
    /// <summary>Id của CaddieBooking cần thanh toán</summary>
    public Guid CaddieBookingId { get; set; }

    /// <summary>Phương thức thanh toán: COD = 0 | BankTransfer = 2</summary>
    public PaymentMethod PaymentMethod { get; set; }
}
