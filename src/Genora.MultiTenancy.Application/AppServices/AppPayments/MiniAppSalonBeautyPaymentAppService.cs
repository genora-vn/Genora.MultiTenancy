using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
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
/// Xử lý payment flow cho lịch hẹn Salon Beauty.
/// orderId format: {BookingCode}_{unixTimestamp} (BookingCode bắt đầu bằng prefix riêng của Salon — vd "SAL...")
/// để phân biệt với Booking golf ("KH"), FnB ("FNB"), Proshop ("PRO") khi nhận callback.
/// </summary>
public class MiniAppSalonBeautyPaymentAppService : ApplicationService, IMiniAppSalonBeautyPaymentAppService
{
    private readonly IRepository<SalonBeautyBooking, Guid> _bookingRepo;
    private readonly ISettingProvider _settingProvider;
    private readonly ICurrentTenant _currentTenant;
    private readonly VietQrApiClient _vietQr;

    public MiniAppSalonBeautyPaymentAppService(
        IRepository<SalonBeautyBooking, Guid> bookingRepo,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant,
        VietQrApiClient vietQr)
    {
        _bookingRepo     = bookingRepo;
        _settingProvider = settingProvider;
        _currentTenant   = currentTenant;
        _vietQr          = vietQr;
    }

    /// <summary>
    /// Tạo payload đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder().
    /// </summary>
    public async Task<PrepareOrderResult> PrepareOrderAsync(PrepareSalonBeautyBookingInput input)
    {
        // 1. Lấy lịch hẹn
        var booking = await _bookingRepo.FindAsync(input.BookingId)
            ?? throw new UserFriendlyException(AppPaymentErrorCodes.BookingNotFound);

        // 2. Kiểm tra trạng thái — chỉ cho thanh toán khi chưa Paid và chưa bị Cancelled
        if (booking.PaymentStatus == SalonBeautyPaymentStatus.Paid)
            throw new UserFriendlyException(AppPaymentErrorCodes.BookingAlreadyPaid);

        if (booking.Status == SalonBeautyBookingStatus.Cancelled)
            throw new UserFriendlyException(AppPaymentErrorCodes.BookingCancelled);

        // 3. Lấy cấu hình payment từ Setting per-tenant
        var appId      = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.AppId) ?? string.Empty;
        var privateKey = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKey))
            throw new UserFriendlyException(AppPaymentErrorCodes.PaymentNotConfigured);

        // 4. Tạo orderId: BookingCode + timestamp
        var orderId     = $"{booking.BookingCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var amount      = (long)booking.TotalAmount;
        var description = $"Thanh toan dat lich Salon - {booking.BookingCode}";

        // 5. Tạo MAC: appId|orderId|amount
        var mac = ZaloMacHelper.GenerateCreateOrderMac(privateKey, appId, orderId, amount);

        // 6. Cập nhật PaymentMethod lên Booking (ghi nhận phương thức khách chọn)
        booking.PaymentMethod = input.PaymentMethod;
        await _bookingRepo.UpdateAsync(booking, autoSave: true);

        // 7. Build kết quả
        var result = new PrepareOrderResult
        {
            AppId             = appId,
            OrderId           = orderId,
            Amount            = amount,
            Description       = description,
            Mac               = mac,
            PaymentMethodName = GetPaymentMethodName(input.PaymentMethod),
        };

        // 8. Đính kèm thông tin ngân hàng + VietQR nếu là BankTransfer
        if (input.PaymentMethod == SalonBeautyPaymentMethod.BankTransfer)
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

                var qrResult = await _vietQr.GenerateAsync(qrRequest)
                            ?? _vietQr.BuildFallback(qrRequest);

                bankInfo.QrCode     = qrResult.QrCode;
                bankInfo.QrImageUrl = qrResult.QrImageUrl;
                bankInfo.BankAppUrl = qrResult.BankAppUrl;
            }

            result.BankInfo = bankInfo;
        }

        return result;
    }

    /// <summary>
    /// Mini App poll kiểm tra trạng thái giao dịch SalonBooking.
    /// orderId format: {BookingCode}_{timestamp}
    /// </summary>
    public async Task<CheckTransactionResult> CheckTransactionAsync(string orderId)
    {
        var bookingCode = ExtractOrderCode(orderId);

        var booking = await _bookingRepo.FindAsync(x => x.BookingCode == bookingCode);
        if (booking == null)
            return new CheckTransactionResult
            {
                OrderId = orderId,
                Status  = PaymentOrderStatus.Failed,
                IsPaid  = false,
                Message = "Không tìm thấy lịch hẹn",
            };

        if (booking.Status == SalonBeautyBookingStatus.Cancelled)
            return new CheckTransactionResult
            {
                OrderId = orderId,
                Status  = PaymentOrderStatus.Cancelled,
                IsPaid  = false,
                Message = "Lịch hẹn đã bị hủy",
            };

        var status = booking.PaymentStatus switch
        {
            SalonBeautyPaymentStatus.Paid     => PaymentOrderStatus.Success,
            SalonBeautyPaymentStatus.Refunded => PaymentOrderStatus.Cancelled,
            _                                  => PaymentOrderStatus.Pending,
        };

        return new CheckTransactionResult
        {
            OrderId = orderId,
            Status  = status,
            IsPaid  = booking.PaymentStatus == SalonBeautyPaymentStatus.Paid,
            Message = status == PaymentOrderStatus.Success ? "Đã thanh toán" : "Chưa thanh toán",
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        // orderId = "SAL2604010001_1743638400" → "SAL2604010001"
        var lastUnderscore = orderId.LastIndexOf('_');
        return lastUnderscore > 0 ? orderId[..lastUnderscore] : orderId;
    }

    private static string GetPaymentMethodName(SalonBeautyPaymentMethod method) => method switch
    {
        SalonBeautyPaymentMethod.Cash         => "Thanh toán tại quầy",
        SalonBeautyPaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        SalonBeautyPaymentMethod.Card         => "Thanh toán bằng thẻ",
        _                                      => "Không xác định",
    };
}
