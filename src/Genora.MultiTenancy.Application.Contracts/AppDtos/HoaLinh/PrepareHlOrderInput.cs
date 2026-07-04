using System;

namespace Genora.MultiTenancy.AppDtos.HoaLinh;

/// <summary>
/// Input chuẩn bị thanh toán đơn hàng Hoa Linh từ Mini App
/// </summary>
public class PrepareHlOrderInput
{
    /// <summary>Id đơn hàng trên Genora (AppHlOrders)</summary>
    public Guid OrderId { get; set; }

    /// <summary>Phương thức thanh toán: 1=Cash, 2=BankTransfer</summary>
    public int PaymentMethod { get; set; }
}
