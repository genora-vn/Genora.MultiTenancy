using Genora.MultiTenancy.Enums;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Kết quả truy vấn trạng thái một đơn hàng thanh toán.
/// Dùng cho cả Booking (đặt sân) và FnbOrder (đặt món).
/// </summary>
public class GetOrderStatusResult
{
    /// <summary>OrderId đã dùng khi gọi createOrder (format: {Code}_{timestamp})</summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Mã đơn hàng nghiệp vụ (VD: KH000001 hoặc FNB2604010001)</summary>
    public string OrderCode { get; set; } = string.Empty;

    /// <summary>Loại đơn hàng: "Booking" hoặc "FnbOrder"</summary>
    public string OrderType { get; set; } = string.Empty;

    /// <summary>Số tiền (VND)</summary>
    public long Amount { get; set; }

    /// <summary>Phương thức thanh toán đã chọn</summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>Tên hiển thị của phương thức thanh toán</summary>
    public string PaymentMethodName { get; set; } = string.Empty;

    /// <summary>Trạng thái thanh toán: Unpaid / Paid / Failed</summary>
    public string PaymentStatus { get; set; } = string.Empty;

    /// <summary>Đã thanh toán xong chưa</summary>
    public bool IsPaid { get; set; }

    public string Message { get; set; } = string.Empty;
}

/// <summary>
/// Input để merchant/admin cập nhật trạng thái thanh toán thủ công.
/// Chỉ áp dụng với COD, BankTransfer.
/// </summary>
public class UpdateFnbPaymentStatusInput
{
    /// <summary>Mã đơn FnB (VD: FNB2604010001)</summary>
    public string FnbOrderCode { get; set; } = string.Empty;

    /// <summary>Trạng thái mới: Paid = 2, Failed = 3</summary>
    public FnbPaymentStatus NewPaymentStatus { get; set; }

    /// <summary>Phương thức thanh toán thực tế (ghi đè nếu cần)</summary>
    public PaymentMethod? PaymentMethod { get; set; }

    /// <summary>Ghi chú nội bộ (ai xác nhận, số bill...)</summary>
    public string? Note { get; set; }
}

/// <summary>
/// Kết quả sau khi cập nhật trạng thái thanh toán FnB.
/// </summary>
public class UpdateFnbPaymentStatusResult
{
    public bool Success { get; set; }
    public string OrderCode { get; set; } = string.Empty;
    public string NewPaymentStatus { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
