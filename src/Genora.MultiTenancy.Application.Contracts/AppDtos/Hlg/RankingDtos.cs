using System;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>
/// Sự kiện xếp hạng kèm GAME hiện tại của sự kiện. Khớp contract RankingEvent.
/// Lưu ý ngữ nghĩa (từ 2026-10-13): <see cref="GameId"/> là game HIỆN TẠI (không còn cố định tuần đầu),
/// <see cref="StartAt"/>/<see cref="EndAt"/> là thời gian của GAME hiện tại.
/// Thời gian gốc của sự kiện (chiến dịch) được giữ ở <see cref="EventStartAt"/>/<see cref="EventEndAt"/> để FE còn dùng.
/// </summary>
public class RankingEventDto
{
    /// <summary>Id GAME hiện tại của sự kiện (ongoing ưu tiên, nếu không có thì ended mới nhất). Null nếu không có game.</summary>
    public Guid? GameId { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    /// <summary>Thời gian bắt đầu của GAME hiện tại (fallback thời gian sự kiện nếu game không cấu hình).</summary>
    public DateTime StartAt { get; set; }
    /// <summary>Thời gian kết thúc của GAME hiện tại (fallback thời gian sự kiện nếu game không cấu hình).</summary>
    public DateTime EndAt { get; set; }
    /// <summary>Tên GAME hiện tại (VD: "Thử tài tốc độ: Dạ Hương Tuần 2").</summary>
    public string? GameName { get; set; }
    /// <summary>Trạng thái GAME hiện tại: "ongoing" | "ended" | "upcoming".</summary>
    public string? GameStatus { get; set; }
    /// <summary>Thời gian bắt đầu gốc của SỰ KIỆN (chiến dịch) — giữ lại cho FE.</summary>
    public DateTime EventStartAt { get; set; }
    /// <summary>Thời gian kết thúc gốc của SỰ KIỆN (chiến dịch) — giữ lại cho FE.</summary>
    public DateTime EventEndAt { get; set; }
}

/// <summary>Một dòng xếp hạng. Khớp contract RankingEntry.</summary>
public class RankingEntryDto
{
    public int Rank { get; set; }
    public Guid UserId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int Score { get; set; }
    public bool IsCurrentUser { get; set; }
}

/// <summary>Kết quả upload ảnh chia sẻ Bảng xếp hạng. FE đọc CHÍNH XÁC data.url (URL HTTPS tuyệt đối, GET công khai).</summary>
public class HlgRankingShareImageResultDto
{
    public string Url { get; set; } = string.Empty;
}
