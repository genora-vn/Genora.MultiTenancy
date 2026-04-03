using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Mini App gửi lên để yêu cầu tạo order thanh toán cho đơn đặt món (FnB).
/// </summary>
public class PrepareFnbOrderInput
{
    /// <summary>Id của FnbOrder cần thanh toán</summary>
    public Guid FnbOrderId { get; set; }

    /// <summary>
    /// Phương thức thanh toán: COD = 0 (tại quầy), BankTransfer = 2 (chuyển khoản).
    /// Online = 1 chỉ dùng khi tích hợp ZaloPay/Momo/VNPay (phase 2).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
}
