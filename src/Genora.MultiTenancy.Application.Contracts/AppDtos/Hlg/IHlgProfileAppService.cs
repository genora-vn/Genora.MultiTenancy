using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Service hồ sơ người chơi Gamification.
/// Tái dùng dbo.AppCustomers (điểm ở Customer.BonusPoint) + bảng HLG.AppHlgUserProfiles cho field game.
/// </summary>
public interface IHlgProfileAppService : IApplicationService
{
    /// <summary>
    /// Đăng ký/đồng bộ khách hàng Gamification vào dbo.AppCustomers + tạo/cập nhật HLG profile.
    /// Idempotent theo phone. customerType gán khi register.
    /// </summary>
    Task<GamificationUserDto> UpsertCustomerAsync(HlgCustomerUpsertPayloadDto payload, CancellationToken ct = default);

    /// <summary>Lấy hồ sơ gamification theo phone (đảm bảo đã tồn tại profile HLG, tạo nếu thiếu).</summary>
    Task<GamificationUserDto> GetByPhoneAsync(string phone, CancellationToken ct = default);

    /// <summary>Cập nhật hồ sơ. Trả về GamificationUser đã cập nhật.</summary>
    Task<GamificationUserDto> UpdateProfileAsync(string phone, UpdateProfilePayloadDto payload, CancellationToken ct = default);

    /// <summary>Thống kê hồ sơ: điểm, số kiến thức đã học, độ chính xác.</summary>
    Task<ProfileStatsDto> GetStatsAsync(string phone, CancellationToken ct = default);

    /// <summary>Lịch sử học kiến thức của người dùng.</summary>
    Task<List<LearningHistoryItemDto>> GetLearningHistoryAsync(string phone, CancellationToken ct = default);

    /// <summary>Lịch sử biến động điểm.</summary>
    Task<List<PointHistoryItemDto>> GetPointHistoryAsync(string phone, CancellationToken ct = default);

    /// <summary>Lịch sử đổi quà.</summary>
    Task<List<RewardHistoryItemDto>> GetRewardHistoryAsync(string phone, CancellationToken ct = default);
}
