using System;

namespace Genora.MultiTenancy.AppDtos.Hlg;

/// <summary>Mục lịch sử học. Khớp contract LearningHistoryItem.</summary>
public class LearningHistoryItemDto
{
    public string? ThumbnailUrl { get; set; }
    public Guid ProductId { get; set; }
    public string ProductName { get; set; } = string.Empty;
    public int ProgressPercent { get; set; }
    public DateTime LastViewedAt { get; set; }
}

/// <summary>Kết quả ghi nhận tiến độ học (server chấm % theo thời gian ở trang + số tab đã xem).</summary>
public class LearningProgressResultDto
{
    public int ProgressPercent { get; set; }
    public bool IsCompleted { get; set; }
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
    public string? GameName { get; set; }
    public Guid Id { get; set; }
    public string RewardName { get; set; } = string.Empty;
    public int PointDelta { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
