namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Dữ liệu trả về cho Mini App để gọi Zalo Checkout SDK createOrder()
/// </summary>
public class PrepareOrderResult
{
    // ── Thông tin truyền vào JS createOrder() ────────────────────────────────
    /// <summary>App ID của Mini App (khớp với cấu hình Zalo)</summary>
    public string AppId { get; set; } = string.Empty;

    /// <summary>Order ID duy nhất — dùng BookingCode + timestamp để tránh trùng</summary>
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Số tiền (VND, không có thập phân)</summary>
    public long Amount { get; set; }

    /// <summary>Mô tả đơn hàng hiển thị trên Zalo Checkout</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>Chữ ký MAC HMAC-SHA256: appId|orderId|amount</summary>
    public string Mac { get; set; } = string.Empty;

    // ── Thông tin bổ sung trả về cho UI ──────────────────────────────────────
    /// <summary>Tên phương thức thanh toán hiển thị</summary>
    public string PaymentMethodName { get; set; } = string.Empty;

    /// <summary>Thông tin ngân hàng (chỉ có khi PaymentMethod = BankTransfer)</summary>
    public BankInfoDto? BankInfo { get; set; }
}

/// <summary>
/// Thông tin tài khoản ngân hàng để hiển thị trên Mini App.
/// Khi PaymentMethod = BankTransfer, các field QR sẽ được điền để
/// Mini App hiển thị QR và nút "Mở app ngân hàng".
/// </summary>
public class BankInfoDto
{
    // ── Thông tin tài khoản ──────────────────────────────────────────────────
    public string BankName      { get; set; } = string.Empty;
    public string AccountNumber { get; set; } = string.Empty;
    public string AccountOwner  { get; set; } = string.Empty;
    public string Branch        { get; set; } = string.Empty;

    // ── VietQR / Thanh toán nhanh ────────────────────────────────────────────

    /// <summary>
    /// Chuỗi QR theo chuẩn EMVCo/VietQR — dùng để render QR code trong Mini App.
    /// VD: "00020101021238560010A000000727..."
    /// </summary>
    public string? QrCode { get; set; }

    /// <summary>
    /// URL ảnh QR từ VietQR CDN — có thể dùng trực tiếp trong thẻ img.
    /// VD: "https://img.vietqr.io/image/TPB-040091011510-qr_only.jpg?amount=2000000&addInfo=..."
    /// </summary>
    public string? QrImageUrl { get; set; }

    /// <summary>
    /// Deep link mở app ngân hàng với số tiền và nội dung chuyển khoản đã điền sẵn.
    /// Mini App dùng cho nút "Mở app ngân hàng".
    /// VD: "https://dl.vietqr.io/pay?app=tpb&ba=040091011510&am=2000000&tn=..."
    /// </summary>
    public string? BankAppUrl { get; set; }
}
