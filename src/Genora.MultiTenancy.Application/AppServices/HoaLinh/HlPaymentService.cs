using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.AppDtos.HoaLinh;
using Genora.MultiTenancy.AppServices.AppPayments;
using Genora.MultiTenancy.DomainModels.AppHlOrders;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.AppServices.HoaLinh;

/// <summary>
/// Service xử lý thanh toán cho đơn hàng Hoa Linh Mini App.
/// Pattern theo MiniAppProPaymentAppService: MAC + VietQR.
/// </summary>
public interface IHlPaymentService
{
    Task<PrepareOrderResult> PrepareOrderAsync(PrepareHlOrderInput input);
    Task<CheckTransactionResult> CheckTransactionAsync(string orderId);
}

public class HlPaymentService : IHlPaymentService
{
    private readonly IRepository<HlOrder, Guid> _orderRepo;
    private readonly ISettingProvider _settings;
    private readonly VietQrApiClient _vietQr;
    private readonly ILogger<HlPaymentService> _logger;

    public HlPaymentService(
        IRepository<HlOrder, Guid> orderRepo,
        ISettingProvider settings,
        VietQrApiClient vietQr,
        ILogger<HlPaymentService> logger)
    {
        _orderRepo = orderRepo;
        _settings = settings;
        _vietQr = vietQr;
        _logger = logger;
    }

    public async Task<PrepareOrderResult> PrepareOrderAsync(PrepareHlOrderInput input)
    {
        // 1. Lấy đơn hàng
        var order = await _orderRepo.FindAsync(input.OrderId)
            ?? throw new UserFriendlyException("Không tìm thấy đơn hàng");

        // 2. Kiểm tra trạng thái
        if (order.DeliveryStatus == HlOrderDeliveryStatus.Cancelled)
            throw new UserFriendlyException("Đơn hàng đã bị hủy");

        if (order.PaymentStatus == HlOrderPaymentStatus.Paid)
            throw new UserFriendlyException("Đơn hàng đã được thanh toán");

        // 3. Lấy cấu hình payment từ Setting per-tenant
        var appId = await _settings.GetOrNullAsync(ZaloPaymentSettingNames.AppId) ?? string.Empty;
        var privateKey = await _settings.GetOrNullAsync(ZaloPaymentSettingNames.PrivateKey) ?? string.Empty;

        if (string.IsNullOrWhiteSpace(appId) || string.IsNullOrWhiteSpace(privateKey))
            throw new UserFriendlyException("Chưa cấu hình thanh toán cho tenant này");

        // 4. Cập nhật PaymentMethod
        order.PaymentMethod = (HlPaymentMethod)input.PaymentMethod;
        await _orderRepo.UpdateAsync(order, autoSave: true);

        // 5. Tạo orderId: {OrderCode}_{timestamp}
         var orderId = $"{order.OrderCode}_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        var amount = (long)order.TotalAmount;
        var description = $"Thanh toan don hang - {order.OrderCode}";

        // 6. Tạo MAC: appId|orderId|amount (dùng ZaloMacHelper)
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
        if (input.PaymentMethod == 2) // BankTransfer
        {
            var bankName = await _settings.GetOrNullAsync(ZaloPaymentSettingNames.BankName) ?? string.Empty;
            var accNumber = await _settings.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountNumber) ?? string.Empty;
            var accOwner = await _settings.GetOrNullAsync(ZaloPaymentSettingNames.BankAccountOwner) ?? string.Empty;
            var branch = await _settings.GetOrNullAsync(ZaloPaymentSettingNames.BankBranch) ?? string.Empty;

            var bankInfo = new BankInfoDto
            {
                BankName = bankName,
                AccountNumber = accNumber,
                AccountOwner = accOwner,
                Branch = branch,
            };

            // Generate VietQR code
            var bankCode = VietQrBankCodeMap.GetCode(bankName);
            if (bankCode.HasValue && !string.IsNullOrWhiteSpace(accNumber))
            {
                var qrRequest = new VietQrRequest
                {
                    BankBin = bankCode.Value.Bin,
                    BankShortCode = bankCode.Value.ShortCode,
                    AccountNumber = accNumber,
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

        _logger.LogInformation("HL PrepareOrder: {OrderId} Amount={Amount} Method={Method}", orderId, amount, input.PaymentMethod);

        return result;
    }

    public async Task<CheckTransactionResult> CheckTransactionAsync(string orderId)
    {
        if (string.IsNullOrWhiteSpace(orderId))
            return new CheckTransactionResult { OrderId = orderId ?? "", Status = PaymentOrderStatus.Failed, Message = "OrderId trống" };

        // Extract order code: orderId = "{OrderCode}_{timestamp}"
        var orderCode = ExtractOrderCode(orderId);

        var queryable = await _orderRepo.GetQueryableAsync();
        var order = queryable.FirstOrDefault(x => x.OrderCode == orderCode);

        if (order == null)
            return new CheckTransactionResult { OrderId = orderId, Status = PaymentOrderStatus.Failed, Message = "Không tìm thấy đơn hàng" };

        if (order.DeliveryStatus == HlOrderDeliveryStatus.Cancelled)
            return new CheckTransactionResult { OrderId = orderId, Status = PaymentOrderStatus.Cancelled, Message = "Đơn hàng đã bị hủy" };

        var isPaid = order.PaymentStatus == HlOrderPaymentStatus.Paid;
        var status = isPaid ? PaymentOrderStatus.Success : PaymentOrderStatus.Pending;

        return new CheckTransactionResult
        {
            OrderId = orderId,
            Status = status,
            IsPaid = isPaid,
            Message = isPaid ? "Đã thanh toán" : "Chờ thanh toán"
        };
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        var lastUnderscore = orderId.LastIndexOf('_');
        return lastUnderscore > 0 ? orderId[..lastUnderscore] : orderId;
    }

    private static string GetPaymentMethodName(int method) => method switch
    {
        1 => "Thanh toán tại quầy",
        2 => "Chuyển khoản ngân hàng",
        _ => "Không xác định",
    };
}
