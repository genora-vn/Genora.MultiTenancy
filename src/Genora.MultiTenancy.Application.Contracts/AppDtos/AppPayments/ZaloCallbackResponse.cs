using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Response trả về cho Zalo Checkout SDK Server sau khi nhận callback
/// </summary>
public class ZaloCallbackResponse
{
    /// <summary>
    /// 1 = Thành công
    /// 2 = Trùng transId (đã xử lý trước đó)
    /// Khác = Thất bại (Zalo sẽ không callback lại)
    /// </summary>
    public int ReturnCode { get; set; }

    /// <summary>Mô tả chi tiết trạng thái</summary>
    public string ReturnMessage { get; set; } = string.Empty;
}

/// <summary>
/// Kết quả kiểm tra trạng thái giao dịch từ Mini App
/// </summary>
public class CheckTransactionResult
{
    public string OrderId    { get; set; } = string.Empty;
    public PaymentOrderStatus Status { get; set; }  // "Pending" | "Success" | "Failed"
    public string Message    { get; set; } = string.Empty;
    public bool   IsPaid     { get; set; }
}
