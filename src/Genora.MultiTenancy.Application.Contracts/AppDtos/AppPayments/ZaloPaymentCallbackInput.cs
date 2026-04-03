using Genora.MultiTenancy.Enums;
using System.Text.Json.Serialization;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Payload Zalo Checkout SDK gửi về server qua Callback URL
/// Tham khảo: https://miniapp.zaloplatforms.com/documents/payment/
/// </summary>
public class ZaloPaymentCallbackInput
{
    /// <summary>Dữ liệu giao dịch từ Zalo</summary>
    [JsonPropertyName("data")]
    public ZaloCallbackData Data { get; set; } = new();

    /// <summary>
    /// MAC xác thực riêng cho trường data.
    /// HMAC-SHA256(privateKey, appId|orderId|transId|amount|description|resultCode|message)
    /// </summary>
    [JsonPropertyName("mac")]
    public string Mac { get; set; } = string.Empty;

    /// <summary>
    /// MAC xác thực toàn bộ payload.
    /// HMAC-SHA256(privateKey, tất cả fields của data sắp xếp theo thứ tự từ điển tăng dần)
    /// </summary>
    [JsonPropertyName("overallMac")]
    public string OverallMac { get; set; } = string.Empty;
}

public class ZaloCallbackData
{
    /// <summary>App ID của Mini App</summary>
    [JsonPropertyName("appId")]
    public string AppId { get; set; } = string.Empty;

    /// <summary>Order ID đã tạo lúc prepareOrder</summary>
    [JsonPropertyName("orderId")]
    public string OrderId { get; set; } = string.Empty;

    /// <summary>Transaction ID từ hệ thống đối tác thanh toán</summary>
    [JsonPropertyName("transId")]
    public string TransId { get; set; } = string.Empty;

    /// <summary>Phương thức thanh toán (mã của Zalo)</summary>
    [JsonPropertyName("method")]
    public string? Method { get; set; }

    /// <summary>Thời gian giao dịch (unix timestamp ms)</summary>
    [JsonPropertyName("transTime")]
    public long? TransTime { get; set; }

    /// <summary>Mã giao dịch của đối tác thanh toán</summary>
    [JsonPropertyName("merchantTransId")]
    public string? MerchantTransId { get; set; }

    /// <summary>Số tiền thanh toán (VND)</summary>
    [JsonPropertyName("amount")]
    public long Amount { get; set; }

    /// <summary>Mô tả đơn hàng</summary>
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    /// <summary>Trạng thái giao dịch: 1 = Thành công, -1 = Thất bại</summary>
    [JsonPropertyName("resultCode")]
    public int ResultCode { get; set; }

    /// <summary>Mô tả resultCode</summary>
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;

    /// <summary>Dữ liệu thêm (đã encodeURIComponent)</summary>
    [JsonPropertyName("extradata")]
    public string? Extradata { get; set; }
}
