using System;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Sự kiện xếp hạng. Khớp contract RankingEvent.</summary>
public class RankingEventDto
{
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
