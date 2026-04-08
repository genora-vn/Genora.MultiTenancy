using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.DomainModels.AppProOrders;
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
/// Xử lý payment flow cho đơn Proshop.
/// orderId format: {ProOrderCode}_{unixTimestamp}  (VD: PRO2604010001_1743638400)
/// ProOrderCode bắt đầu bằng "PRO" → phân biệt với Booking ("KH") và FnB ("FNB") ở callback.
/// </summary>
public class MiniAppProPaymentAppService : ApplicationService, IMiniAppProPaymentAppService
{
    private readonly IRepository<ProOrder, Guid> _proOrderRepo;
    private readonly ISettingProvider _settingProvider;
    private readonly ICurrentTenant _currentTenant;
    private readonly VietQrApiClient _vietQr;

    public MiniAppProPaymentAppService(
        IRepository<ProOrder, Guid> proOrderRepo,
        ISettingProvider settingProvider,
        ICurrentTenant currentTenant,
        VietQrApiClient vietQr)
    {
        _proOrderRepo    = proOrderRepo;
        _settingProvider = settingProvider;
        _currentTenant   = currentTenant;
        _vietQr          = vietQr;
    }

    /// <summary>
    /// Tạo payload đã ký MAC để Mini App gọi Zalo Checkout SDK createOrder().
    /// </summary>
    public async Task<PrepareOrderResult> PrepareOrderAsync(PrepareProOrderInput input)
    {
        // 1. Lấy đơn Proshop
        var order = await _proOrderRepo.FindAsync(input.ProOrderId)
            ?? throw new UserFriendlyException(AppPaymentErrorCodes.OrderNotFound);

        // 2. Kiểm tra trạng thái — chỉ cho thanh toán khi chưa Paid
        if (order.PaymentStatus == ProPaymentStatus.Paid)
            throw new UserFriendlyException(AppPaymentErrorCodes.OrderAlreadyPaid);

        // 3. Lấy cấu hình payment từ Setting per-tenant
        var appId      = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.AppId) ?? string.Empty;
        var privateKey = await _settingProvider.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKey))
            throw new UserFriendlyException(AppPaymentErrorCodes.PaymentNotConfigured);

        // 4. Tạo orderId: ProOrderCode + timestamp
        var orderId     = $"{order.OrderCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var amount      = (long)order.TotalAmount;
        var description = $"Thanh toan proshop - {order.OrderCode}";

        // 5. Tạo MAC: appId|orderId|amount
        var mac = ZaloMacHelper.GenerateCreateOrderMac(privateKey, appId, orderId, amount);

        // 6. Cập nhật PaymentMethod lên ProOrder (ghi nhận phương thức khách chọn)
        order.PaymentMethod = input.PaymentMethod;
        await _proOrderRepo.UpdateAsync(order, autoSave: true);

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
    /// Mini App poll kiểm tra trạng thái giao dịch ProOrder.
    /// orderId format: {ProOrderCode}_{timestamp}
    /// </summary>
    public async Task<CheckTransactionResult> CheckTransactionAsync(string orderId)
    {
        var proOrderCode = ExtractOrderCode(orderId);

        var order = await _proOrderRepo.FindAsync(x => x.OrderCode == proOrderCode);
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
            ProPaymentStatus.Paid   => PaymentOrderStatus.Success,
            ProPaymentStatus.Failed => PaymentOrderStatus.Failed,
            _                       => PaymentOrderStatus.Pending,
        };

        return new CheckTransactionResult
        {
            OrderId = orderId,
            Status  = status,
            IsPaid  = order.PaymentStatus == ProPaymentStatus.Paid,
            Message = status == PaymentOrderStatus.Success ? "Đã thanh toán" : "Chưa thanh toán",
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        // orderId = "PRO2604010001_1743638400" → "PRO2604010001"
        var lastUnderscore = orderId.LastIndexOf('_');
        return lastUnderscore > 0 ? orderId[..lastUnderscore] : orderId;
    }

    private static string GetPaymentMethodName(PaymentMethod method) => method switch
    {
        PaymentMethod.COD          => "Thanh toán tại quầy",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        PaymentMethod.Online       => "Thanh toán online",
        _                          => "Không xác định",
    };
}
