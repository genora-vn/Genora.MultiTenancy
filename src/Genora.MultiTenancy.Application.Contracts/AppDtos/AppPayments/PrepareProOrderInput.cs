using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Mini App gửi lên để yêu cầu tạo order thanh toán cho đơn Proshop.
/// </summary>
public class PrepareProOrderInput
{
    /// <summary>Id của ProOrder cần thanh toán</summary>
    public Guid ProOrderId { get; set; }

    /// <summary>
    /// Phương thức thanh toán: COD = 0 (tại quầy), BankTransfer = 2 (chuyển khoản).
    /// </summary>
    public PaymentMethod PaymentMethod { get; set; }
}
