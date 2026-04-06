using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppBookings;
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
/// Service để Mini App gọi trước khi hiển thị Zalo Checkout SDK.
/// Trả về payload đã ký MAC để Mini App truyền vào createOrder().
/// </summary>
public class MiniAppPaymentAppService : ApplicationService, IMiniAppPaymentAppService
{
    private readonly IRepository<Booking, Guid> _bookingRepo;
    private readonly ISettingProvider            _settingProvider;
    private readonly ICurrentTenant              _currentTenant;
    private readonly VietQrApiClient             _vietQr;

    public MiniAppPaymentAppService(
        IRepository<Booking, Guid> bookingRepo,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant,
        VietQrApiClient vietQr)
    {
        _bookingRepo     = bookingRepo;
        _settingProvider = settingProvider;
        _currentTenant   = currentTenant;
        _vietQr          = vietQr;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // PrepareOrderAsync
    // Mini App gọi API này → nhận payload → gọi JS createOrder()
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<PrepareOrderResult> PrepareOrderAsync(PrepareOrderInput input)
    {
        // 1. Load booking
        var booking = await _bookingRepo.FindAsync(input.BookingId)
            ?? throw new UserFriendlyException(AppPaymentErrorCodes.BookingNotFound);

        if (booking.Status == BookingStatus.Paid || booking.Status == BookingStatus.Completed)
            throw new UserFriendlyException(AppPaymentErrorCodes.BookingAlreadyPaid);

        if (booking.Status == BookingStatus.CancelledRefund || booking.Status == BookingStatus.CancelledNoRefund)
            throw new UserFriendlyException(AppPaymentErrorCodes.BookingCancelled);

        // 2. Load settings per-tenant
        // AppId tái dụng ZaloSettingNames.MiniAppId (đã được cấu hình sẵn trên trang Zalo/ZNS)
        // PrivateKey lưu riêng tại ZaloPaymentSettingNames.PrivateKey (nhập trên cùng trang)
        var appId      = await _settingProvider.GetOrNullAsync(ZaloSettingNames.MiniAppId) ?? string.Empty;
        var privateKey = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKey))
            throw new UserFriendlyException(AppPaymentErrorCodes.PaymentNotConfigured);

        // 3. Tạo orderId duy nhất: BookingCode + timestamp (tránh trùng khi retry)
        var orderId     = $"{booking.BookingCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        decimal? totalAmount = booking.TotalAmount;
        var amount = (long)(totalAmount ?? 0m);
        var description = $"Thanh toan dat san - {booking.BookingCode}";

        // 4. Tạo MAC: appId|orderId|amount
        var mac = ZaloMacHelper.GenerateCreateOrderMac(privateKey, appId, orderId, amount);

        // 5. Build result
        var result = new PrepareOrderResult
        {
            AppId             = appId,
            OrderId           = orderId,
            Amount            = amount,
            Description       = description,
            Mac               = mac,
            PaymentMethodName = input.PaymentMethod == PaymentMethod.COD
                ? "Thanh toán khi nhận hàng (COD)"
                : "Chuyển khoản ngân hàng"
        };

        // 6. Nếu là BankTransfer → đính kèm thông tin tài khoản + VietQR
        if (input.PaymentMethod == PaymentMethod.BankTransfer)
        {
            var bankName  = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankName)          ?? string.Empty;
            var accountNo = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountNumber) ?? string.Empty;
            var accOwner  = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountOwner)  ?? string.Empty;
            var branch    = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.BankBranch)        ?? string.Empty;

            var bankInfo = new BankInfoDto
            {
                BankName      = bankName,
                AccountNumber = accountNo,
                AccountOwner  = accOwner,
                Branch        = branch,
            };

            // Tạo QR + deeplink nếu có thể map được mã ngân hàng
            var bankCode = VietQrBankCodeMap.GetCode(bankName);
            if (bankCode.HasValue && !string.IsNullOrWhiteSpace(accountNo))
            {
                var qrRequest = new VietQrRequest
                {
                    BankBin       = bankCode.Value.Bin,
                    BankShortCode = bankCode.Value.ShortCode,
                    AccountNumber = accountNo,
                    AccountOwner  = accOwner,
                    Amount        = amount,
                    AddInfo       = description,
                };

                // Gọi VietQR API lấy chuỗi QR — fallback sang imageUrl nếu API lỗi
                var qrResult = await _vietQr.GenerateAsync(qrRequest)
                            ?? _vietQr.BuildFallback(qrRequest);

                bankInfo.QrCode     = qrResult.QrCode;
                bankInfo.QrImageUrl = qrResult.QrImageUrl;
                bankInfo.BankAppUrl = qrResult.BankAppUrl;
            }

            result.BankInfo = bankInfo;
        }

        // 7. Cập nhật PaymentMethod trên Booking (lưu phương thức đã chọn)
        booking.PaymentMethod = input.PaymentMethod;
        await _bookingRepo.UpdateAsync(booking, autoSave: true);

        return result;
    }

    // ─────────────────────────────────────────────────────────────────────────
    // CheckTransactionAsync
    // Mini App gọi sau khi createOrder() hoàn tất để lấy trạng thái
    // orderId = BookingCode_timestamp → tách lấy BookingCode để tra cứu
    // ─────────────────────────────────────────────────────────────────────────
    public async Task<CheckTransactionResult> CheckTransactionAsync(string orderId)
    {
        // orderId format: {BookingCode}_{timestamp}
        var bookingCode = orderId.Contains('_')
            ? orderId[..orderId.LastIndexOf('_')]
            : orderId;

        var booking = await _bookingRepo.FindAsync(x => x.BookingCode == bookingCode);

        if (booking == null)
            return new CheckTransactionResult
            {
                OrderId = orderId,
                Status  = PaymentOrderStatus.Failed,
                Message = "Không tìm thấy booking",
                IsPaid  = false
            };

        var isPaid = booking.Status == BookingStatus.Paid || booking.Status == BookingStatus.Completed;

        var status = isPaid
            ? PaymentOrderStatus.Success
            : booking.Status == BookingStatus.CancelledRefund || booking.Status == BookingStatus.CancelledNoRefund
                ? PaymentOrderStatus.Cancelled
                : PaymentOrderStatus.Pending;

        return new CheckTransactionResult
        {
            OrderId = orderId,
            Status  = status,
            Message = isPaid ? "Thanh toán thành công" : "Chờ xác nhận thanh toán",
            IsPaid  = isPaid
        };
    }
}
