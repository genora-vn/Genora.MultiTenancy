using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Xử lý Notify từ Zalo Checkout SDK khi người dùng chọn COD hoặc BankTransfer.
///
/// Notify ≠ Callback:
///   - Notify → gọi ngay khi user XÁC NHẬN phương thức thanh toán (chưa thực sự trả tiền)
///   - Callback → gọi sau khi giao dịch HOÀN TẤT (tiền đã chuyển / COD đã xác nhận giao)
///
/// Với COD và BankTransfer, Notify ghi nhận phương thức và giữ nguyên PaymentStatus = Unpaid.
/// Merchant sẽ gọi UpdatePaymentStatus khi đã nhận tiền thực tế.
/// </summary>
public class PaymentNotifyAppService : ApplicationService, IPaymentNotifyAppService
{
    private readonly IRepository<Booking, Guid>     _bookingRepo;
    private readonly IRepository<FnbOrder, Guid>    _fnbOrderRepo;
    private readonly ISettingProvider               _settingProvider;

    public PaymentNotifyAppService(
        IRepository<Booking, Guid>  bookingRepo,
        IRepository<FnbOrder, Guid> fnbOrderRepo,
        ISettingProvider            settingProvider)
    {
        _bookingRepo      = bookingRepo;
        _fnbOrderRepo     = fnbOrderRepo;
        _settingProvider  = settingProvider;
    }

    public async Task<ZaloCallbackResponse> HandleNotifyAsync(ZaloPaymentCallbackInput input)
    {
        var data = input.Data;

        try
        {
            // ── 1. Lấy PrivateKey per-tenant ─────────────────────────────────
            var privateKey = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;
            if (string.IsNullOrWhiteSpace(privateKey))
            {
                Logger.LogWarning("[ZaloPaymentNotify] PrivateKey chưa cấu hình, bỏ qua notify orderId={OrderId}", data.OrderId);
                return Fail("PrivateKey not configured");
            }

            // ── 2. Verify MAC ─────────────────────────────────────────────────
            var expectedMac = ZaloMacHelper.GenerateCallbackMac(
                privateKey, data.AppId, data.OrderId, data.TransId,
                data.Amount, data.Description, data.ResultCode, data.Message);

            if (!string.Equals(input.Mac, expectedMac, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning("[ZaloPaymentNotify] MAC không hợp lệ, orderId={OrderId}", data.OrderId);
                return Fail("Invalid MAC");
            }

            // ── 3. Nhận biết loại đơn qua prefix của orderId ─────────────────
            var orderCode = ExtractOrderCode(data.OrderId);

            if (orderCode.StartsWith("FNB", StringComparison.OrdinalIgnoreCase))
                return await HandleFnbNotifyAsync(orderCode, data);
            else
                return await HandleBookingNotifyAsync(orderCode, data);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "[ZaloPaymentNotify] Lỗi xử lý notify orderId={OrderId}", data.OrderId);
            return Fail(ex.Message);
        }
    }

    // ── FnbOrder Notify ───────────────────────────────────────────────────────

    private async Task<ZaloCallbackResponse> HandleFnbNotifyAsync(string fnbOrderCode, ZaloCallbackData data)
    {
        var order = await _fnbOrderRepo.FindAsync(x => x.OrderCode == fnbOrderCode);
        if (order == null)
        {
            Logger.LogWarning("[ZaloPaymentNotify] FnbOrder không tìm thấy, code={Code}", fnbOrderCode);
            return Fail("Order not found");
        }

        // Ghi nhận phương thức thanh toán từ method do Zalo trả về
        // COD: method = "cod", BankTransfer: method = "bank_transfer"
        if (!string.IsNullOrWhiteSpace(data.Method))
        {
            order.PaymentMethod = MapZaloMethod(data.Method);
            await _fnbOrderRepo.UpdateAsync(order, autoSave: true);
        }

        Logger.LogInformation("[ZaloPaymentNotify] FnbOrder {Code} notify OK, method={Method}", fnbOrderCode, data.Method);
        return Ok();
    }

    // ── Booking Notify ────────────────────────────────────────────────────────

    private async Task<ZaloCallbackResponse> HandleBookingNotifyAsync(string bookingCode, ZaloCallbackData data)
    {
        var booking = await _bookingRepo.FindAsync(x => x.BookingCode == bookingCode);
        if (booking == null)
        {
            Logger.LogWarning("[ZaloPaymentNotify] Booking không tìm thấy, code={Code}", bookingCode);
            return Fail("Booking not found");
        }

        // Ghi nhận phương thức — không thay đổi status (chưa thanh toán thực tế)
        if (!string.IsNullOrWhiteSpace(data.Method))
        {
            booking.PaymentMethod = MapZaloMethod(data.Method);
            await _bookingRepo.UpdateAsync(booking, autoSave: true);
        }

        Logger.LogInformation("[ZaloPaymentNotify] Booking {Code} notify OK, method={Method}", bookingCode, data.Method);
        return Ok();
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        var idx = orderId.LastIndexOf('_');
        return idx > 0 ? orderId[..idx] : orderId;
    }

    private static PaymentMethod MapZaloMethod(string method) => method.ToLowerInvariant() switch
    {
        "cod"           => PaymentMethod.COD,
        "bank_transfer" => PaymentMethod.BankTransfer,
        _               => PaymentMethod.Online,
    };

    private static ZaloCallbackResponse Ok()   => new() { ReturnCode = 1, ReturnMessage = "Success" };
    private static ZaloCallbackResponse Fail(string msg) => new() { ReturnCode = -1, ReturnMessage = msg };
}
