using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.Hlg;

/// <summary>
/// Phần thưởng Gamification: danh sách quà, đổi quà (trừ Customer.BonusPoint), địa chỉ giao hàng, lịch sử.
/// Phân luồng nhận quà theo customerType (BD-3, BD-6): pharmacy vs consumer.
/// Điểm trừ vào Customer.BonusPoint (AD-2). Internal service — controller gọi trực tiếp.
/// </summary>
[RemoteService(false)]
[DisableValidation]
public class HlgRewardAppService : ApplicationService, IHlgRewardAppService
{
    private readonly IRepository<HlgReward, Guid> _rewardRepo;
    private readonly IRepository<HlgRewardHistory, Guid> _historyRepo;
    private readonly IRepository<HlgShippingAddress, Guid> _shippingRepo;
    private readonly IRepository<HlgGameSession, Guid> _sessionRepo;
    private readonly IRepository<HlgUserProfile, Guid> _profileRepo;
    private readonly IRepository<Customer, Guid> _customerRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IUnitOfWorkManager _uowManager;
    private readonly ILogger<HlgRewardAppService> _logger;

    public HlgRewardAppService(
        IRepository<HlgReward, Guid> rewardRepo,
        IRepository<HlgRewardHistory, Guid> historyRepo,
        IRepository<HlgShippingAddress, Guid> shippingRepo,
        IRepository<HlgGameSession, Guid> sessionRepo,
        IRepository<HlgUserProfile, Guid> profileRepo,
        IRepository<Customer, Guid> customerRepo,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager uowManager,
        ILogger<HlgRewardAppService> logger)
    {
        _rewardRepo = rewardRepo;
        _historyRepo = historyRepo;
        _shippingRepo = shippingRepo;
        _sessionRepo = sessionRepo;
        _profileRepo = profileRepo;
        _customerRepo = customerRepo;
        _currentTenant = currentTenant;
        _uowManager = uowManager;
        _logger = logger;
    }

    public async Task<List<RewardDto>> GetRewardsAsync(CancellationToken ct = default)
    {
        var q = await _rewardRepo.GetQueryableAsync();
        var rewards = await AsyncExecuter.ToListAsync(
            q.Where(x => x.IsActive && (x.StockQuantity == null || x.StockQuantity > 0))
             .OrderBy(x => x.DisplayOrder).ThenBy(x => x.PointCost), ct);

        return rewards.Select(MapReward).ToList();
    }

    public async Task<RewardHistoryItemDto> RedeemAsync(Guid rewardId, string phone, Guid? shippingAddressId = null, CancellationToken ct = default)
    {
        var customer = await ResolveCustomerAsync(phone, ct);

        // ACID: trừ điểm + tạo history + giảm tồn kho trong 1 transaction.
        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: true);

        var reward = await _rewardRepo.FirstOrDefaultAsync(x => x.Id == rewardId && x.IsActive, ct)
            ?? throw new UserFriendlyException("Không tìm thấy phần thưởng");

        if (reward.StockQuantity is <= 0)
            throw new UserFriendlyException("Phần thưởng đã hết");

        // Kiểm tra đủ điểm.
        if (customer.BonusPoint < reward.PointCost)
            throw new UserFriendlyException("Bạn không đủ điểm để đổi phần thưởng này");

        // Xác định luồng theo customerType (BD-6).
        var profile = await _profileRepo.FirstOrDefaultAsync(x => x.CustomerId == customer.Id, ct);
        var isConsumer = profile?.CustomerType == HlgCustomerType.Consumer;

        // Quà physical cho consumer bắt buộc có địa chỉ.
        if (reward.Type == HlgRewardType.Physical && isConsumer && shippingAddressId == null)
            throw new UserFriendlyException("Vui lòng cung cấp địa chỉ giao hàng để nhận quà");

        // Trừ điểm.
        customer.BonusPoint -= reward.PointCost;
        await _customerRepo.UpdateAsync(customer, autoSave: true, cancellationToken: ct);

        // Giảm tồn kho (nếu có giới hạn).
        if (reward.StockQuantity.HasValue)
        {
            reward.StockQuantity -= 1;
            await _rewardRepo.UpdateAsync(reward, autoSave: true, cancellationToken: ct);
        }

        // Trạng thái ban đầu theo loại quà.
        var status = reward.Type switch
        {
            HlgRewardType.Voucher => HlgRewardHistoryStatus.Done,   // voucher cấp ngay (nối UrBox ở bước tích hợp)
            HlgRewardType.Physical when isConsumer => HlgRewardHistoryStatus.Shipping,
            _ => HlgRewardHistoryStatus.Pending
        };

        var history = new HlgRewardHistory(GuidGenerator.Create(), customer.Id, reward.Id, reward.Name, _currentTenant.Id)
        {
            PointDelta = -reward.PointCost,
            RewardType = reward.Type,
            Status = status,
            ShippingAddressId = shippingAddressId,
            VoucherCode = reward.Type == HlgRewardType.Voucher ? reward.VoucherCode : null
        };
        history = await _historyRepo.InsertAsync(history, autoSave: true, cancellationToken: ct);

        await uow.CompleteAsync(ct);

        _logger.LogInformation("HLG redeem: customer {CustomerId} đổi quà {RewardId} (-{Points}đ) status={Status}",
            customer.Id, reward.Id, reward.PointCost, status);

        return MapHistory(history);
    }

    public async Task SetSessionShippingAddressAsync(Guid sessionId, ShippingAddressPayloadDto payload, CancellationToken ct = default)
    {
        if (payload == null
            || string.IsNullOrWhiteSpace(payload.ReceiverName)
            || string.IsNullOrWhiteSpace(payload.Phone)
            || string.IsNullOrWhiteSpace(payload.Address))
        {
            throw new UserFriendlyException("Thiếu thông tin địa chỉ giao hàng (tên, sđt, địa chỉ)");
        }

        var session = await _sessionRepo.FirstOrDefaultAsync(x => x.Id == sessionId, ct)
            ?? throw new UserFriendlyException("Không tìm thấy phiên chơi");

        var address = new HlgShippingAddress(
            GuidGenerator.Create(),
            session.CustomerId,
            payload.ReceiverName!.Trim(),
            NormalizePhone(payload.Phone)!,
            payload.Address!.Trim(),
            _currentTenant.Id)
        {
            Note = string.IsNullOrWhiteSpace(payload.Note) ? null : payload.Note!.Trim()
        };
        address = await _shippingRepo.InsertAsync(address, autoSave: true, cancellationToken: ct);

        session.ShippingAddressId = address.Id;
        await _sessionRepo.UpdateAsync(session, autoSave: true, cancellationToken: ct);

        _logger.LogInformation("HLG: lưu địa chỉ ship cho session {SessionId} customer {CustomerId}",
            sessionId, session.CustomerId);
    }

    public async Task<List<RewardHistoryItemDto>> GetRewardHistoryAsync(string phone, CancellationToken ct = default)
    {
        var customer = await ResolveCustomerAsync(phone, ct);

        var q = await _historyRepo.GetQueryableAsync();
        var rows = await AsyncExecuter.ToListAsync(
            q.Where(x => x.CustomerId == customer.Id).OrderByDescending(x => x.CreationTime), ct);

        return rows.Select(MapHistory).ToList();
    }

    // ── Helpers ────────────────────────────────────────────────────────────

    private async Task<Customer> ResolveCustomerAsync(string phone, CancellationToken ct)
    {
        var normalized = NormalizePhone(phone);
        if (string.IsNullOrWhiteSpace(normalized))
            throw new UserFriendlyException("Thiếu số điện thoại");

        return await _customerRepo.FirstOrDefaultAsync(x => x.PhoneNumber == normalized, ct)
            ?? throw new UserFriendlyException("Không tìm thấy khách hàng. Vui lòng đăng ký trước.");
    }

    private static RewardDto MapReward(HlgReward r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        ImageUrl = r.ImageUrl,
        PointCost = r.PointCost,
        Type = HlgEnumMapper.RewardTypeToString(r.Type)
    };

    private static RewardHistoryItemDto MapHistory(HlgRewardHistory h) => new()
    {
        Id = h.Id,
        RewardName = h.RewardName,
        PointDelta = h.PointDelta,
        Status = HlgEnumMapper.RewardHistoryStatusToString(h.Status),
        CreatedAt = h.CreationTime
    };

    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone)) return null;
        return Regex.Replace(phone.Trim(), @"\s+|-|\.", "");
    }
}
