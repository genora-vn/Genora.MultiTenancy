using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppFnbOrderActivity;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Xử lý callback từ Zalo Checkout SDK Server.
/// Phân loại đơn hàng tự động:
///   - OrderCode bắt đầu "FNB" → FnbOrder (đặt món)
///   - Còn lại (VD: "KH")      → Booking (đặt sân)
///
/// Endpoint KHÔNG cần JWT — Zalo server gọi trực tiếp.
/// Bảo mật hoàn toàn bằng verify MAC/overallMac.
/// </summary>
public class PaymentCallbackAppService : ApplicationService, IPaymentCallbackAppService
{
    private readonly IRepository<Booking, Guid>          _bookingRepo;
    private readonly IRepository<FnbOrder, Guid>         _fnbOrderRepo;
    private readonly IRepository<FnbOrderActivity, Guid> _activityRepo;
    private readonly ISettingProvider                    _settingProvider;
    private readonly ICurrentTenant                      _currentTenant;

    public PaymentCallbackAppService(
        IRepository<Booking, Guid>          bookingRepo,
        IRepository<FnbOrder, Guid>         fnbOrderRepo,
        IRepository<FnbOrderActivity, Guid> activityRepo,
        ISettingProvider                    settingProvider,
        ICurrentTenant                      currentTenant)
    {
        _bookingRepo     = bookingRepo;
        _fnbOrderRepo    = fnbOrderRepo;
        _activityRepo    = activityRepo;
        _settingProvider = settingProvider;
        _currentTenant   = currentTenant;
    }

    public async Task<ZaloCallbackResponse> HandleCallbackAsync(ZaloPaymentCallbackInput input)
    {
        var data = input.Data;

        Logger.LogInformation(
            "[ZaloPaymentCallback] orderId={OrderId} transId={TransId} resultCode={ResultCode} amount={Amount}",
            data.OrderId, data.TransId, data.ResultCode, data.Amount);

        // ── 1. Load Private Key per-tenant ────────────────────────────────────
        var privateKey      = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;
        var configuredAppId = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.AppId)     ?? string.Empty;

        if (string.IsNullOrWhiteSpace(privateKey))
        {
            Logger.LogWarning("[ZaloPaymentCallback] PrivateKey chưa cấu hình, tenantId={TenantId}", _currentTenant.Id);
            return Fail("Payment not configured");
        }

        // ── 2. Verify AppId ───────────────────────────────────────────────────
        if (!string.IsNullOrWhiteSpace(configuredAppId) &&
            !string.Equals(data.AppId, configuredAppId, StringComparison.OrdinalIgnoreCase))
        {
            Logger.LogWarning("[ZaloPaymentCallback] AppId mismatch: received={R} configured={C}", data.AppId, configuredAppId);
            return Fail(AppPaymentErrorCodes.AppIdMismatch);
        }

        // ── 3. Verify MAC ─────────────────────────────────────────────────────
        if (!ZaloMacHelper.VerifyCallbackMac(privateKey, input.Mac,
                data.AppId, data.OrderId, data.TransId,
                data.Amount, data.Description, data.ResultCode, data.Message))
        {
            Logger.LogWarning("[ZaloPaymentCallback] MAC invalid, orderId={OrderId}", data.OrderId);
            return Fail(AppPaymentErrorCodes.InvalidMac);
        }

        // ── 4. Verify overallMac ──────────────────────────────────────────────
        if (!ZaloMacHelper.VerifyOverallMac(privateKey, input.OverallMac, BuildOverallFields(data)))
        {
            Logger.LogWarning("[ZaloPaymentCallback] OverallMAC invalid, orderId={OrderId}", data.OrderId);
            return Fail(AppPaymentErrorCodes.InvalidMac);
        }

        // ── 5. Nhận biết loại đơn qua OrderCode prefix ───────────────────────
        var orderCode = ExtractOrderCode(data.OrderId);

        return orderCode.StartsWith("FNB", StringComparison.OrdinalIgnoreCase)
            ? await HandleFnbCallbackAsync(orderCode, data)
            : await HandleBookingCallbackAsync(orderCode, data);
    }

    // ─────────────────────────────────────────────────────────────────────────
    // FnbOrder callback
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<ZaloCallbackResponse> HandleFnbCallbackAsync(string fnbOrderCode, ZaloCallbackData data)
    {
        var order = await _fnbOrderRepo.FindAsync(x => x.OrderCode == fnbOrderCode);
        if (order == null)
        {
            Logger.LogWarning("[ZaloPaymentCallback] FnbOrder không tìm thấy, code={Code}", fnbOrderCode);
            return Fail(AppPaymentErrorCodes.OrderNotFound);
        }

        // Kiểm tra amount
        if ((long)order.TotalAmount != data.Amount)
        {
            Logger.LogWarning("[ZaloPaymentCallback] FnbOrder amount mismatch: received={R} stored={S}", data.Amount, order.TotalAmount);
            return Fail(AppPaymentErrorCodes.InvalidAmount);
        }

        // Idempotent: đã Paid rồi thì trả về returnCode=2
        if (order.PaymentStatus == FnbPaymentStatus.Paid)
        {
            Logger.LogInformation("[ZaloPaymentCallback] FnbOrder {Code} đã Paid trước đó (duplicate)", fnbOrderCode);
            return new ZaloCallbackResponse { ReturnCode = 2, ReturnMessage = "Duplicate transaction" };
        }

        if (data.ResultCode == 1)
        {
            // Thanh toán thành công
            var oldStatus = order.PaymentStatus;
            order.PaymentStatus = FnbPaymentStatus.Paid;
            await _fnbOrderRepo.UpdateAsync(order, autoSave: true);

            // Ghi activity
            await _activityRepo.InsertAsync(new FnbOrderActivity(
                id:          GuidGenerator.Create(),
                orderId:     order.Id,
                actionType:  "PaymentStatusChanged",
                title:       $"Thanh toán: {oldStatus} → Paid",
                description: $"Zalo Checkout callback thành công. TransId={data.TransId}",
                actionTime:  DateTime.UtcNow,
                isDanger:    false,
                tenantId:    _currentTenant.Id
            ), autoSave: true);

            Logger.LogInformation("[ZaloPaymentCallback] FnbOrder {Code} → Paid. TransId={TransId}", fnbOrderCode, data.TransId);
            return new ZaloCallbackResponse { ReturnCode = 1, ReturnMessage = "Success" };
        }
        else
        {
            // Thanh toán thất bại
            order.PaymentStatus = FnbPaymentStatus.Failed;
            await _fnbOrderRepo.UpdateAsync(order, autoSave: true);

            await _activityRepo.InsertAsync(new FnbOrderActivity(
                id:          GuidGenerator.Create(),
                orderId:     order.Id,
                actionType:  "PaymentStatusChanged",
                title:       "Thanh toán thất bại",
                description: $"Zalo Checkout callback thất bại. Message={data.Message}",
                actionTime:  DateTime.UtcNow,
                isDanger:    true,
                tenantId:    _currentTenant.Id
            ), autoSave: true);

            Logger.LogWarning("[ZaloPaymentCallback] FnbOrder {Code} payment FAILED. Message={Msg}", fnbOrderCode, data.Message);
            return new ZaloCallbackResponse { ReturnCode = 0, ReturnMessage = data.Message };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Booking callback (giữ nguyên logic cũ)
    // ─────────────────────────────────────────────────────────────────────────
    private async Task<ZaloCallbackResponse> HandleBookingCallbackAsync(string bookingCode, ZaloCallbackData data)
    {
        var booking = await _bookingRepo.FindAsync(x => x.BookingCode == bookingCode);
        if (booking == null)
        {
            Logger.LogWarning("[ZaloPaymentCallback] Booking không tìm thấy, code={Code}", bookingCode);
            return Fail(AppPaymentErrorCodes.OrderNotFound);
        }

        decimal? totalAmount = booking.TotalAmount;
        var storedAmount = (long)(totalAmount ?? 0m);
        if (storedAmount != data.Amount)
        {
            Logger.LogWarning("[ZaloPaymentCallback] Booking amount mismatch: received={R} stored={S}", data.Amount, storedAmount);
            return Fail(AppPaymentErrorCodes.InvalidAmount);
        }

        if (booking.Status == BookingStatus.Paid || booking.Status == BookingStatus.Completed)
        {
            Logger.LogInformation("[ZaloPaymentCallback] Booking {Code} đã Paid trước đó (duplicate)", bookingCode);
            return new ZaloCallbackResponse { ReturnCode = 2, ReturnMessage = "Duplicate transaction" };
        }

        if (data.ResultCode == 1)
        {
            booking.Status = BookingStatus.Paid;
            await _bookingRepo.UpdateAsync(booking, autoSave: true);

            Logger.LogInformation("[ZaloPaymentCallback] Booking {Code} → Paid. TransId={TransId}", bookingCode, data.TransId);
            return new ZaloCallbackResponse { ReturnCode = 1, ReturnMessage = "Success" };
        }
        else
        {
            Logger.LogWarning("[ZaloPaymentCallback] Booking {Code} payment FAILED. Message={Msg}", bookingCode, data.Message);
            return new ZaloCallbackResponse { ReturnCode = 0, ReturnMessage = data.Message };
        }
    }

    // ─────────────────────────────────────────────────────────────────────────
    // Helpers
    // ─────────────────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        var idx = orderId.LastIndexOf('_');
        return idx > 0 ? orderId[..idx] : orderId;
    }

    private static Dictionary<string, string> BuildOverallFields(ZaloCallbackData data)
    {
        var fields = new Dictionary<string, string>
        {
            ["appId"]       = data.AppId,
            ["orderId"]     = data.OrderId,
            ["transId"]     = data.TransId,
            ["amount"]      = data.Amount.ToString(),
            ["description"] = data.Description,
            ["resultCode"]  = data.ResultCode.ToString(),
            ["message"]     = data.Message,
        };

        if (data.TransTime.HasValue)
            fields["transTime"] = data.TransTime.Value.ToString();
        if (!string.IsNullOrWhiteSpace(data.Method))
            fields["method"] = data.Method;
        if (!string.IsNullOrWhiteSpace(data.MerchantTransId))
            fields["merchantTransId"] = data.MerchantTransId;
        if (!string.IsNullOrWhiteSpace(data.Extradata))
            fields["extradata"] = data.Extradata;

        return fields;
    }

    private static ZaloCallbackResponse Fail(string msg)
        => new() { ReturnCode = -1, ReturnMessage = msg };
}
