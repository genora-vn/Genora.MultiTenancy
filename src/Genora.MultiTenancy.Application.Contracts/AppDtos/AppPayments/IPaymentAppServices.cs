using Genora.MultiTenancy.AppDtos.AppPayments;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.AppPayments;

/// <summary>
/// Interface cho MiniApp gọi payment flow — Đặt sân Golf (Booking)
/// </summary>
public interface IMiniAppPaymentAppService
{
    /// <summary>
    /// Tạo payload đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder().
    /// Booking: orderId = {BookingCode}_{unixTimestamp}
    /// </summary>
    Task<PrepareOrderResult> PrepareOrderAsync(PrepareOrderInput input);

    /// <summary>
    /// Mini App poll để kiểm tra trạng thái giao dịch Booking sau khi createOrder().
    /// </summary>
    Task<CheckTransactionResult> CheckTransactionAsync(string orderId);
}

/// <summary>
/// Interface cho MiniApp gọi payment flow — Đặt món FnB (FnbOrder)
/// </summary>
public interface IMiniAppFnbPaymentAppService
{
    /// <summary>
    /// Tạo payload đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder() cho đơn FnB.
    /// FnbOrder: orderId = {FnbOrderCode}_{unixTimestamp}
    /// </summary>
    Task<PrepareOrderResult> PrepareOrderAsync(PrepareFnbOrderInput input);

    /// <summary>
    /// Mini App poll kiểm tra trạng thái giao dịch FnbOrder sau khi createOrder().
    /// </summary>
    Task<CheckTransactionResult> CheckTransactionAsync(string orderId);
}

/// <summary>
/// Xử lý Callback từ Zalo Checkout SDK Server (sau khi giao dịch hoàn tất).
/// Áp dụng cho cả Booking và FnbOrder — phân biệt qua orderId prefix (KH/FNB).
/// </summary>
public interface IPaymentCallbackAppService
{
    Task<ZaloCallbackResponse> HandleCallbackAsync(ZaloPaymentCallbackInput input);
}

/// <summary>
/// Xử lý Notify từ Zalo Checkout SDK khi người dùng chọn COD hoặc BankTransfer.
/// Notify được gọi TRƯỚC callback — để thông báo phương thức đã chọn.
/// Áp dụng cho cả Booking và FnbOrder.
/// </summary>
public interface IPaymentNotifyAppService
{
    Task<ZaloCallbackResponse> HandleNotifyAsync(ZaloPaymentCallbackInput input);
}

/// <summary>
/// Truy vấn và cập nhật trạng thái thanh toán thủ công.
/// Dành cho merchant/admin xác nhận COD hoặc BankTransfer đã nhận tiền.
/// </summary>
public interface IFnbOrderStatusAppService
{
    /// <summary>Truy vấn trạng thái thanh toán theo orderId (format: {Code}_{timestamp})</summary>
    Task<GetOrderStatusResult> GetOrderStatusAsync(string orderId);

    /// <summary>
    /// Merchant xác nhận đã nhận tiền → cập nhật PaymentStatus = Paid.
    /// Chỉ áp dụng với COD, BankTransfer.
    /// </summary>
    Task<UpdateFnbPaymentStatusResult> UpdatePaymentStatusAsync(UpdateFnbPaymentStatusInput input);
}
