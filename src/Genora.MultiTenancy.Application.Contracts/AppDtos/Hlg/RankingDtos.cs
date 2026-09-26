using System;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Sự kiện xếp hạng. Khớp contract RankingEvent.</summary>
public class RankingEventDto
{
    public Guid? GameId { get; set; }
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime StartAt { get; set; }
    public DateTime EndAt { get; set; }
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
