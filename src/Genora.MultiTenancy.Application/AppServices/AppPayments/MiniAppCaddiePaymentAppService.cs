using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Enums.ErrorCodes;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Xử lý payment flow cho đặt Caddie.
/// orderId format: {BookingCode}_{unixTimestamp}  (VD: CB-20260604-FDB7_1743638400)
/// BookingCode bắt đầu bằng "CB-" → phân biệt với Booking/FnB/Pro ở callback.
/// </summary>
public class MiniAppCaddiePaymentAppService : ApplicationService, IMiniAppCaddiePaymentAppService
{
    private readonly IRepository<AppCaddieBooking, Guid> _bookingRepo;
    private readonly IRepository<GolfCourse, Guid> _golfCourseRepo;
    private readonly ISettingProvider _settingProvider;
    private readonly ICurrentTenant _currentTenant;
    private readonly VietQrApiClient _vietQr;

    public MiniAppCaddiePaymentAppService(
        IRepository<AppCaddieBooking, Guid> bookingRepo,
        IRepository<GolfCourse, Guid> golfCourseRepo,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant,
        VietQrApiClient vietQr)
    {
        _bookingRepo = bookingRepo;
        _golfCourseRepo = golfCourseRepo;
        _settingProvider = settingProvider;
        _currentTenant = currentTenant;
        _vietQr = vietQr;
    }

    /// <summary>
    /// Tạo payload đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder().
    /// </summary>
    public async Task<PrepareOrderResult> PrepareOrderAsync(PrepareCaddieBookingInput input)
    {
        // 1. Lấy booking caddie
        var booking = await _bookingRepo.FindAsync(input.CaddieBookingId)
            ?? throw new UserFriendlyException(AppPaymentErrorCodes.OrderNotFound);

        // 2. Kiểm tra trạng thái — chỉ cho thanh toán khi chưa Paid
        if (booking.PaymentStatus == (byte)CaddiePaymentStatus.Paid)
            throw new UserFriendlyException(AppPaymentErrorCodes.OrderAlreadyPaid);

        // 3. Lấy phí caddie từ GolfCourse
        var golfCourse = await _golfCourseRepo.FindAsync(booking.GolfCourseId);
        var caddieFee = golfCourse?.CaddieFee ?? 0;
        var amount = (long)caddieFee;

        if (amount <= 0)
            throw new UserFriendlyException("Chưa cấu hình phí dịch vụ Caddie cho sân golf này.");

        // 4. Lấy cấu hình payment từ Setting per-tenant
        var appId = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.AppId) ?? string.Empty;
        var privateKey = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKey))
            throw new UserFriendlyException(AppPaymentErrorCodes.PaymentNotConfigured);

        // 5. Tạo orderId: BookingCode + timestamp
        var orderId = $"{booking.BookingCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var description = $"Thanh toan dich vu Caddie - {booking.BookingCode}";

        // 6. Tạo MAC: appId|orderId|amount
        var mac = ZaloMacHelper.GenerateCreateOrderMac(privateKey, appId, orderId, amount);

        // 7. Build kết quả
        var result = new PrepareOrderResult
        {
            AppId = appId,
            OrderId = orderId,
            Amount = amount,
            Description = description,
            Mac = mac,
            PaymentMethodName = GetPaymentMethodName(input.PaymentMethod),
        };

        // 8. Đính kèm thông tin ngân hàng + VietQR nếu là BankTransfer
        if (input.PaymentMethod == PaymentMethod.BankTransfer)
        {
            var bankName = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankName) ?? string.Empty;
            var accountNo = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountNumber) ?? string.Empty;
            var accOwner = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountOwner) ?? string.Empty;
            var branch = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankBranch) ?? string.Empty;

            var bankInfo = new BankInfoDto
            {
                BankName = bankName,
                AccountNumber = accountNo,
                AccountOwner = accOwner,
                Branch = branch,
            };

            var bankCode = VietQrBankCodeMap.GetCode(bankName);
            if (bankCode.HasValue && !string.IsNullOrWhiteSpace(accountNo))
            {
                var qrRequest = new VietQrRequest
                {
                    BankBin = bankCode.Value.Bin,
                    BankShortCode = bankCode.Value.ShortCode,
                    AccountNumber = accountNo,
                    AccountOwner = accOwner,
                    Amount = amount,
                    AddInfo = description,
                };

                var qrResult = await _vietQr.GenerateAsync(qrRequest)
                            ?? _vietQr.BuildFallback(qrRequest);

                bankInfo.QrCode = qrResult.QrCode;
                bankInfo.QrImageUrl = qrResult.QrImageUrl;
                bankInfo.BankAppUrl = qrResult.BankAppUrl;
            }

            result.BankInfo = bankInfo;
        }

        return result;
    }

    /// <summary>
    /// Mini App poll kiểm tra trạng thái giao dịch CaddieBooking.
    /// orderId format: {BookingCode}_{timestamp}
    /// </summary>
    public async Task<CheckTransactionResult> CheckTransactionAsync(string orderId)
    {
        // Tách BookingCode từ orderId
        var bookingCode = ExtractOrderCode(orderId);

        var booking = await _bookingRepo.FindAsync(x => x.BookingCode == bookingCode);
        if (booking == null)
            return new CheckTransactionResult
            {
                OrderId = orderId,
                Status = PaymentOrderStatus.Failed,
                IsPaid = false,
                Message = "Không tìm thấy booking caddie",
            };

        var status = booking.PaymentStatus switch
        {
            (byte)CaddiePaymentStatus.Paid => PaymentOrderStatus.Success,
            _ => PaymentOrderStatus.Pending,
        };

        return new CheckTransactionResult
        {
            OrderId = orderId,
            Status = status,
            IsPaid = booking.PaymentStatus == (byte)CaddiePaymentStatus.Paid,
            Message = status == PaymentOrderStatus.Success ? "Đã thanh toán" : "Chưa thanh toán",
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        // orderId = "CB-20260604-FDB7_1743638400" → "CB-20260604-FDB7"
        var lastUnderscore = orderId.LastIndexOf('_');
        return lastUnderscore > 0 ? orderId[..lastUnderscore] : orderId;
    }

    private static string GetPaymentMethodName(PaymentMethod method) => method switch
    {
        PaymentMethod.COD => "Thanh toán tại quầy",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        PaymentMethod.Online => "Thanh toán online",
        _ => "Không xác định",
    };
}
