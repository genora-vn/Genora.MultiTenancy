using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
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
/// Xử lý payment flow cho đơn đặt món FnB.
/// orderId format: {FnbOrderCode}_{unixTimestamp}  (VD: FNB2604010001_1743638400)
/// FnbOrderCode bắt đầu bằng "FNB" → phân biệt với Booking ("KH") ở callback.
/// </summary>
public class MiniAppFnbPaymentAppService : ApplicationService, IMiniAppFnbPaymentAppService
{
    private readonly IRepository<FnbOrder, Guid> _fnbOrderRepo;
    private readonly ISettingProvider _settingProvider;
    private readonly ICurrentTenant _currentTenant;
    private readonly VietQrApiClient _vietQr;

    public MiniAppFnbPaymentAppService(
        IRepository<FnbOrder, Guid> fnbOrderRepo,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant,
        VietQrApiClient vietQr)
    {
        _fnbOrderRepo    = fnbOrderRepo;
        _settingProvider = settingProvider;
        _currentTenant   = currentTenant;
        _vietQr          = vietQr;
    }

    /// <summary>
    /// Tạo payload đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder().
    /// </summary>
    public async Task<PrepareOrderResult> PrepareOrderAsync(PrepareFnbOrderInput input)
    {
        // 1. Lấy đơn FnB
        var order = await _fnbOrderRepo.FindAsync(input.FnbOrderId)
            ?? throw new UserFriendlyException(AppPaymentErrorCodes.OrderNotFound);

        // 2. Kiểm tra trạng thái — chỉ cho thanh toán khi chưa Paid
        if (order.PaymentStatus == FnbPaymentStatus.Paid)
            throw new UserFriendlyException(AppPaymentErrorCodes.OrderAlreadyPaid);

        // 3. Lấy cấu hình payment từ Setting per-tenant
        //    AppId dùng chung ZaloSettingNames.MiniAppId (= ZaloPaymentSettingNames.AppId)
        var appId      = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.AppId) ?? string.Empty;
        var privateKey = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKey))
            throw new UserFriendlyException(AppPaymentErrorCodes.PaymentNotConfigured);

        // 4. Tạo orderId: FnbOrderCode + timestamp (FnbOrderCode bắt đầu "FNB" → dễ nhận biết ở callback)
        var orderId     = $"{order.OrderCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var amount      = (long)order.TotalAmount;
        var description = $"Thanh toan dat mon - {order.OrderCode}";

        // 5. Tạo MAC: appId|orderId|amount
        var mac = ZaloMacHelper.GenerateCreateOrderMac(privateKey, appId, orderId, amount);

        // 6. Cập nhật PaymentMethod lên FnbOrder (ghi nhận phương thức khách chọn)
        order.PaymentMethod = input.PaymentMethod;
        await _fnbOrderRepo.UpdateAsync(order, autoSave: true);

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
    /// Mini App poll kiểm tra trạng thái giao dịch FnbOrder.
    /// orderId format: {FnbOrderCode}_{timestamp}
    /// </summary>
    public async Task<CheckTransactionResult> CheckTransactionAsync(string orderId)
    {
        // Tách FnbOrderCode từ orderId
        var fnbOrderCode = ExtractOrderCode(orderId);

        var order = await _fnbOrderRepo.FindAsync(x => x.OrderCode == fnbOrderCode);
        if (order == null)
            return new CheckTransactionResult
            {
                OrderId = orderId,
                Status  = PaymentOrderStatus.Failed,
                IsPaid  = false,
                Message = "Không tìm thấy đơn hàng",
            };

        var status = order.PaymentStatus switch
        {
            FnbPaymentStatus.Paid   => PaymentOrderStatus.Success,
            FnbPaymentStatus.Failed => PaymentOrderStatus.Failed,
            _                       => PaymentOrderStatus.Pending,
        };

        return new CheckTransactionResult
        {
            OrderId = orderId,
            Status  = status,
            IsPaid  = order.PaymentStatus == FnbPaymentStatus.Paid,
            Message = status == PaymentOrderStatus.Success ? "Đã thanh toán" : "Chưa thanh toán",
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        // orderId = "FNB2604010001_1743638400" → "FNB2604010001"
        var lastUnderscore = orderId.LastIndexOf('_');
        return lastUnderscore > 0 ? orderId[..lastUnderscore] : orderId;
    }

    private static string GetPaymentMethodName(PaymentMethod method) => method switch
    {
        PaymentMethod.COD         => "Thanh toán tại quầy",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        PaymentMethod.Online      => "Thanh toán online",
        _                          => "Không xác định",
    };
}
