using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Mini App Salon Beauty gửi lên để yêu cầu tạo order thanh toán cho booking lịch hẹn.
/// </summary>
public class PrepareSalonBeautyBookingInput
{
    /// <summary>Id của SalonBeautyBooking cần thanh toán</summary>
    public Guid BookingId { get; set; }

    /// <summary>
    /// Phương thức thanh toán: Cash = 1 (tại quầy), BankTransfer = 2 (chuyển khoản).
    /// Card = 3 dành cho luồng quẹt thẻ tại quầy (chưa hỗ trợ ở Mini App phase này).
    /// </summary>
    public SalonBeautyPaymentMethod PaymentMethod { get; set; }
}
