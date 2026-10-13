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
    /// Bảng xếp hạng theo TỪNG GAME (chặng) trong sự kiện: chỉ tính phiên ĐẠT của game đó.
    /// Dùng cho trao giải theo game đã kết thúc.
    /// </summary>
    Task<List<RankingEntryDto>> GetGameEntriesAsync(Guid eventId, Guid gameId, string? phone = null, int top = 50, CancellationToken ct = default);

    /// <summary>
    /// Lưu ảnh chia sẻ Bảng xếp hạng do FE chụp (PNG/JPEG), trả URL HTTPS công khai để dùng làm thumbnail share.
    /// Server tự sinh UUID + validate nội dung ảnh thật (không tin extension/MIME). phone là định danh khách hàng (không phải chứng cứ auth).
    /// Ném UserFriendlyException với Code = HTTP status ("400"/"404"/"413"/"500").
    /// </summary>
    Task<HlgRankingShareImageResultDto> SaveShareImageAsync(string phone, byte[]? content, CancellationToken ct = default);
    /// <summary>
    /// Thông tin GAME để render header Bảng xếp hạng.
    /// gameId: nếu truyền → trả ĐÚNG game đó (không ràng buộc event đang active — xem được cả game đã kết thúc);
    /// game không tồn tại/sai tenant → null; game mồ côi (không thuộc event) → trả field game, field event trống.
    /// Nếu null → giữ hành vi cũ: game HIỆN HÀNH của sự kiện active (ongoing ưu tiên → ended mới nhất); không có → null.
    /// </summary>
    Task<RankingEventDto?> GetCurrentEventAsync(Guid? gameId = null, CancellationToken ct = default);

    /// <summary>
    /// Bảng xếp hạng theo GAME trong sự kiện hiện tại. Tính rank (ORDER BY điểm) + đánh dấu isCurrentUser theo phone.
    /// gameId: nếu truyền → xếp hạng CHỈ của game đó; nếu null → dùng "game hiện tại" của sự kiện
    /// (ongoing ưu tiên, nếu không có thì ended mới nhất). Không có game hiện tại → danh sách rỗng.
    /// top: số lượng dòng trả về (mặc định 50); luôn kèm dòng của user hiện tại nếu ngoài top.
    /// </summary>
    Task<List<RankingEntryDto>> GetEntriesAsync(string? phone = null, int top = 50, Guid? gameId = null, CancellationToken ct = default);
}
