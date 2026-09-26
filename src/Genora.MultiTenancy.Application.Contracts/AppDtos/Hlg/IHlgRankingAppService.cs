using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Service xếp hạng Gamification (BD-5).
/// Ranking reset theo sự kiện: điểm tính từ các phiên game finish trong khoảng [StartAt, EndAt].
/// </summary>
public interface IHlgRankingAppService : IApplicationService
{
    Task<List<RankingEntryDto>> GetEventEntriesAsync(Guid eventId, string? phone = null, int top = 50, CancellationToken ct = default);

    /// <summary>
    /// Lưu ảnh chia sẻ Bảng xếp hạng do FE chụp (PNG/JPEG), trả URL HTTPS công khai để dùng làm thumbnail share.
    /// Server tự sinh UUID + validate nội dung ảnh thật (không tin extension/MIME). phone là định danh khách hàng (không phải chứng cứ auth).
    /// Ném UserFriendlyException với Code = HTTP status ("400"/"404"/"413"/"500").
    /// </summary>
    Task<HlgRankingShareImageResultDto> SaveShareImageAsync(string phone, byte[]? content, CancellationToken ct = default);
    /// <summary>Sự kiện xếp hạng đang kích hoạt hiện tại (mới nhất còn hiệu lực). Null nếu không có.</summary>
    Task<RankingEventDto?> GetCurrentEventAsync(CancellationToken ct = default);

    /// <summary>
    /// Bảng xếp hạng của sự kiện hiện tại. Tính rank (ORDER BY điểm) + đánh dấu isCurrentUser theo phone.
    /// top: số lượng dòng trả về (mặc định 50); luôn kèm dòng của user hiện tại nếu ngoài top.
    /// </summary>
    Task<List<RankingEntryDto>> GetEntriesAsync(string? phone = null, int top = 50, CancellationToken ct = default);
}
