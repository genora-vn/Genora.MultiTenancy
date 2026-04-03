using Genora.MultiTenancy.AppDtos.AppPayments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Controllers;

/// <summary>
/// Các endpoints phía server liên quan đến Zalo Checkout SDK.
///
/// ⚠️  Callback và Notify KHÔNG dùng [Authorize] — Zalo server gọi trực tiếp.
///     Bảo mật hoàn toàn bằng verify MAC/overallMac trong từng AppService.
///
/// URLs cần cấu hình trên Zalo Developer Portal:
///   Callback Staging:    https://staging.genora.vn/api/payment/callback
///   Callback Production: https://genora.vn/api/payment/callback
///   Notify Staging:      https://staging.genora.vn/api/payment/notify
///   Notify Production:   https://genora.vn/api/payment/notify
/// </summary>
[Area("MultiTenancy")]
[Route("api/payment")]
[AllowAnonymous]
public class PaymentCallbackController : AbpController
{
    private readonly IPaymentCallbackAppService  _callbackService;
    private readonly IPaymentNotifyAppService    _notifyService;
    private readonly IFnbOrderStatusAppService   _fnbStatusService;

    public PaymentCallbackController(
        IPaymentCallbackAppService  callbackService,
        IPaymentNotifyAppService    notifyService,
        IFnbOrderStatusAppService   fnbStatusService)
    {
        _callbackService  = callbackService;
        _notifyService    = notifyService;
        _fnbStatusService = fnbStatusService;
    }

    // ── Callback ──────────────────────────────────────────────────────────────

    /// <summary>
    /// Nhận callback từ Zalo Checkout SDK Server sau khi giao dịch hoàn tất.
    /// Áp dụng cho cả Booking (đặt sân) và FnbOrder (đặt món).
    /// POST /api/payment/callback
    /// </summary>
    [HttpPost("callback")]
    [IgnoreAntiforgeryToken]
    public Task<ZaloCallbackResponse> Callback([FromBody] ZaloPaymentCallbackInput input)
        => _callbackService.HandleCallbackAsync(input);

    // ── Notify ────────────────────────────────────────────────────────────────

    /// <summary>
    /// Nhận Notify từ Zalo Checkout SDK khi người dùng xác nhận phương thức COD hoặc BankTransfer.
    /// Notify gọi TRƯỚC callback — chỉ thông báo phương thức đã chọn, chưa xác nhận thanh toán.
    /// Áp dụng cho cả Booking và FnbOrder.
    /// POST /api/payment/notify
    /// </summary>
    [HttpPost("notify")]
    [IgnoreAntiforgeryToken]
    public Task<ZaloCallbackResponse> Notify([FromBody] ZaloPaymentCallbackInput input)
        => _notifyService.HandleNotifyAsync(input);

    // ── Order Status ──────────────────────────────────────────────────────────

    /// <summary>
    /// Truy vấn trạng thái thanh toán của một đơn hàng FnB.
    /// orderId format: {FnbOrderCode}_{timestamp}  (VD: FNB2604010001_1743638400)
    /// GET /api/payment/order-status/{orderId}
    /// </summary>
    [HttpGet("order-status/{orderId}")]
    public Task<GetOrderStatusResult> GetOrderStatus(string orderId)
        => _fnbStatusService.GetOrderStatusAsync(orderId);

    // ── Update Payment Status (Merchant/Admin) ────────────────────────────────

    /// <summary>
    /// Merchant/Admin xác nhận đã nhận tiền → cập nhật PaymentStatus = Paid cho FnbOrder.
    /// Chỉ áp dụng với COD và BankTransfer (không phải Online gateway).
    /// Yêu cầu đăng nhập — override AllowAnonymous ở class level bằng [Authorize].
    /// POST /api/payment/update-payment-status
    /// </summary>
    [HttpPost("update-payment-status")]
    [Authorize]
    [IgnoreAntiforgeryToken]
    public Task<UpdateFnbPaymentStatusResult> UpdatePaymentStatus([FromBody] UpdateFnbPaymentStatusInput input)
        => _fnbStatusService.UpdatePaymentStatusAsync(input);
}
