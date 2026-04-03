using Genora.MultiTenancy.AppDtos.AppPayments;
using Genora.MultiTenancy.DomainModels.AppFnbOrderActivity;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Enums.ErrorCodes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppPayments;

/// <summary>
/// Truy vấn và cập nhật trạng thái thanh toán thủ công cho FnbOrder.
///
/// GetOrderStatus  → Mini App / Merchant truy vấn (anonymous OK vì orderId là token ngắn hạn)
/// UpdatePaymentStatus → Merchant/Admin xác nhận đã nhận tiền (yêu cầu xác thực)
/// </summary>
public class FnbOrderStatusAppService : ApplicationService, IFnbOrderStatusAppService
{
    private readonly IRepository<FnbOrder, Guid>         _fnbOrderRepo;
    private readonly IRepository<FnbOrderActivity, Guid> _activityRepo;
    private readonly ICurrentTenant                      _currentTenant;

    public FnbOrderStatusAppService(
        IRepository<FnbOrder, Guid>         fnbOrderRepo,
        IRepository<FnbOrderActivity, Guid> activityRepo,
        ICurrentTenant                      currentTenant)
    {
        _fnbOrderRepo  = fnbOrderRepo;
        _activityRepo  = activityRepo;
        _currentTenant = currentTenant;
    }

    /// <summary>
    /// Truy vấn trạng thái thanh toán theo orderId (format: {FnbOrderCode}_{timestamp}).
    /// </summary>
    public async Task<GetOrderStatusResult> GetOrderStatusAsync(string orderId)
    {
        var orderCode = ExtractOrderCode(orderId);

        var order = await _fnbOrderRepo.FindAsync(x => x.OrderCode == orderCode);
        if (order == null)
            return new GetOrderStatusResult
            {
                OrderId     = orderId,
                OrderCode   = orderCode,
                OrderType   = "FnbOrder",
                Message     = "Không tìm thấy đơn hàng",
                PaymentStatus = "NotFound",
                IsPaid      = false,
            };

        return new GetOrderStatusResult
        {
            OrderId           = orderId,
            OrderCode         = order.OrderCode,
            OrderType         = "FnbOrder",
            Amount            = (long)order.TotalAmount,
            PaymentMethod     = order.PaymentMethod,
            PaymentMethodName = GetMethodName(order.PaymentMethod),
            PaymentStatus     = order.PaymentStatus.ToString(),
            IsPaid            = order.PaymentStatus == FnbPaymentStatus.Paid,
            Message           = order.PaymentStatus == FnbPaymentStatus.Paid
                                    ? "Đã thanh toán"
                                    : "Chưa thanh toán",
        };
    }

    /// <summary>
    /// Merchant/Admin xác nhận thanh toán thủ công (COD, BankTransfer).
    /// Chỉ dùng cho FnbOrder. Yêu cầu đăng nhập [Authorize].
    /// </summary>
    [Authorize]
    public async Task<UpdateFnbPaymentStatusResult> UpdatePaymentStatusAsync(UpdateFnbPaymentStatusInput input)
    {
        if (string.IsNullOrWhiteSpace(input.FnbOrderCode))
            throw new UserFriendlyException(AppPaymentErrorCodes.OrderNotFound);

        var order = await _fnbOrderRepo.FindAsync(x => x.OrderCode == input.FnbOrderCode)
            ?? throw new UserFriendlyException(AppPaymentErrorCodes.OrderNotFound);

        // ── Validation ───────────────────────────────────────────────────────
        if (order.PaymentStatus == FnbPaymentStatus.Paid)
            throw new UserFriendlyException(AppPaymentErrorCodes.OrderAlreadyPaid);

        // Chỉ cho phép update với COD, BankTransfer (không phải Online gateway)
        var effectiveMethod = input.PaymentMethod ?? order.PaymentMethod;
        if (effectiveMethod == PaymentMethod.Online)
            throw new UserFriendlyException("Chỉ cập nhật thủ công với COD hoặc Chuyển khoản.");

        var oldStatus = order.PaymentStatus;

        // ── Cập nhật ─────────────────────────────────────────────────────────
        order.PaymentStatus = input.NewPaymentStatus;
        if (input.PaymentMethod.HasValue)
            order.PaymentMethod = input.PaymentMethod.Value;

        await _fnbOrderRepo.UpdateAsync(order, autoSave: true);

        // ── Ghi Activity ─────────────────────────────────────────────────────
        var isDanger = input.NewPaymentStatus == FnbPaymentStatus.Failed;
        var activity = new FnbOrderActivity(
            id:          GuidGenerator.Create(),
            orderId:     order.Id,
            actionType:  "PaymentStatusChanged",
            title:       $"Thanh toán: {oldStatus} → {input.NewPaymentStatus}",
            description: string.IsNullOrWhiteSpace(input.Note)
                             ? $"Cập nhật bởi {CurrentUser.UserName ?? "admin"}"
                             : input.Note,
            actionTime:  DateTime.UtcNow,
            isDanger:    isDanger,
            tenantId:    _currentTenant.Id
        );
        await _activityRepo.InsertAsync(activity, autoSave: true);

        Logger.LogInformation(
            "[FnbPaymentUpdate] Order={Code} {Old}→{New} by {User}",
            order.OrderCode, oldStatus, input.NewPaymentStatus, CurrentUser.UserName);

        return new UpdateFnbPaymentStatusResult
        {
            Success          = true,
            OrderCode        = order.OrderCode,
            NewPaymentStatus = input.NewPaymentStatus.ToString(),
            Message          = input.NewPaymentStatus == FnbPaymentStatus.Paid
                                   ? "Đã xác nhận thanh toán thành công"
                                   : "Đã cập nhật trạng thái thanh toán",
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private static string ExtractOrderCode(string orderId)
    {
        var idx = orderId.LastIndexOf('_');
        return idx > 0 ? orderId[..idx] : orderId;
    }

    private static string GetMethodName(PaymentMethod? method) => method switch
    {
        PaymentMethod.COD          => "Thanh toán tại quầy",
        PaymentMethod.BankTransfer => "Chuyển khoản ngân hàng",
        PaymentMethod.Online       => "Thanh toán online",
        _                          => "Chưa chọn",
    };
}
