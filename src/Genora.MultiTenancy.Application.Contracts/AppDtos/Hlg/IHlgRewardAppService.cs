using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Service phần thưởng Gamification: danh sách quà, đổi quà (trừ BonusPoint),
/// địa chỉ giao hàng (luồng consumer), lịch sử đổi quà.
/// Phân luồng nhận quà theo customerType (BD-3, BD-6): pharmacy vs consumer.
/// </summary>
public interface IHlgRewardAppService : IApplicationService
{
    /// <summary>Danh sách quà có thể đổi (active, còn tồn kho).</summary>
    Task<List<RewardDto>> GetRewardsAsync(CancellationToken ct = default);

    /// <summary>
    /// Đổi điểm lấy quà. Trừ Customer.BonusPoint, ghi HlgRewardHistory.
    /// - voucher: phát mã (nối UrBox ở bước tích hợp), status Done.
    /// - physical + consumer: cần địa chỉ ship (shippingAddressId), status Pending→Shipping.
    /// - physical + pharmacy: nhận tại nhà thuốc, status Pending.
    /// </summary>
    Task<RewardHistoryItemDto> RedeemAsync(Guid rewardId, string phone, Guid? shippingAddressId = null, CancellationToken ct = default);

    /// <summary>
    /// Lưu địa chỉ giao hàng cho một phiên game (luồng consumer nhận quà vật lý sau game).
    /// Endpoint: POST games/sessions/{sessionId}/shipping-address.
    /// </summary>
    Task SetSessionShippingAddressAsync(Guid sessionId, ShippingAddressPayloadDto payload, CancellationToken ct = default);

    /// <summary>Lịch sử đổi quà của người dùng (theo phone). Dùng cho endpoint profile/reward-history.</summary>
    Task<List<RewardHistoryItemDto>> GetRewardHistoryAsync(string phone, CancellationToken ct = default);
}
