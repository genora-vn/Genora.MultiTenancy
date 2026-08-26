using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>Input chung cho báo cáo (lọc theo khoảng thời gian).</summary>
public class Hl25ReportInput
{
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }

    /// <summary>Lọc theo chiến dịch (dùng cho báo cáo Frame). Null = tất cả.</summary>
    public Guid? CampaignId { get; set; }
}

// ===== Báo cáo 1: Đổi Frame =====

/// <summary>Một dòng thống kê tạo thiệp theo ngày.</summary>
public class Hl25FrameStatsRowDto
{
    /// <summary>Ngày (yyyy-MM-dd).</summary>
    public string Date { get; set; } = null!;

    /// <summary>Số lượt tạo thiệp trong ngày.</summary>
    public int CreationCount { get; set; }

    /// <summary>Số lượt đã chia sẻ trong ngày.</summary>
    public int SharedCount { get; set; }
}

/// <summary>Kết quả báo cáo tạo thiệp (Frame).</summary>
public class Hl25FrameStatsDto
{
    /// <summary>Tổng lượt tạo thiệp.</summary>
    public int TotalCreations { get; set; }

    /// <summary>Tổng lượt đã chia sẻ.</summary>
    public int TotalShared { get; set; }

    /// <summary>Số người tham gia tạo thiệp (distinct).</summary>
    public int UniqueParticipants { get; set; }

    /// <summary>Chi tiết theo ngày.</summary>
    public List<Hl25FrameStatsRowDto> ByDate { get; set; } = new();
}

// ===== Báo cáo 2: Tham gia Vòng quay =====

/// <summary>Kết quả báo cáo tham gia vòng quay.</summary>
public class Hl25WheelParticipationStatsDto
{
    /// <summary>Số người đã quay (distinct participant có ít nhất 1 lượt quay).</summary>
    public int UniqueSpinners { get; set; }

    /// <summary>Tổng số lượt quay đã thực hiện.</summary>
    public int TotalSpins { get; set; }

    /// <summary>Tổng số lượt quay đã cấp (từ SpinTurnLog).</summary>
    public int TotalTurnsGranted { get; set; }

    /// <summary>Số lượt trúng quà (RewardStatus != NotWon).</summary>
    public int TotalWins { get; set; }

    /// <summary>Số người tham gia có ít nhất 1 lượt còn lại chưa quay.</summary>
    public int ParticipantsWithRemainingTurns { get; set; }
}

// ===== Báo cáo 3: Vòng quay theo Quà =====

/// <summary>Một dòng cơ cấu giải theo quà.</summary>
public class Hl25WheelGiftStatsRowDto
{
    public Guid GiftId { get; set; }
    public string GiftName { get; set; } = null!;

    /// <summary>Tổng số lượng quà (kho).</summary>
    public int TotalQuantity { get; set; }

    /// <summary>Số lượng còn lại.</summary>
    public int RemainingQuantity { get; set; }

    /// <summary>Số lượt trúng quà này (tổng).</summary>
    public int WonCount { get; set; }

    /// <summary>Số lượt đã trao (Delivered).</summary>
    public int DeliveredCount { get; set; }

    /// <summary>Tỷ lệ trúng (%) trên tổng số lượt trúng của tất cả quà.</summary>
    public decimal WinRatePercent { get; set; }
}

/// <summary>Kết quả báo cáo vòng quay theo quà.</summary>
public class Hl25WheelGiftStatsDto
{
    /// <summary>Tổng số lượt trúng (mọi quà).</summary>
    public int TotalWon { get; set; }

    /// <summary>Chi tiết theo từng quà.</summary>
    public List<Hl25WheelGiftStatsRowDto> Rows { get; set; } = new();
}
