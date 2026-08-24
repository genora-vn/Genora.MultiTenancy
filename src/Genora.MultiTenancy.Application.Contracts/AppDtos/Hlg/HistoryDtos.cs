using System;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Mục lịch sử học. Khớp contract LearningHistoryItem.</summary>
public class LearningHistoryItemDto
{
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public DateTime LastViewedAt { get; set; }
}

/// <summary>Mục lịch sử điểm. Khớp contract PointHistoryItem.</summary>
public class PointHistoryItemDto
{
    public Guid Id { get; set; }
    public string SourceName { get; set; } = string.Empty;
    public int PointDelta { get; set; }
    public DateTime CreatedAt { get; set; }
}

/// <summary>Mục lịch sử đổi quà. Khớp contract RewardHistoryItem. status: pending|shipping|delivered|done.</summary>
public class RewardHistoryItemDto
{
    public Guid Id { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public int PointDelta { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
