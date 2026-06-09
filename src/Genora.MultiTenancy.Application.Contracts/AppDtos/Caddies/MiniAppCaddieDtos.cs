using System;
using System.Collections.Generic;
using Genora.MultiTenancy.AppDtos.AppZaloAuths;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class MiniAppCaddieListDto
{
    public Guid Id { get; set; }
    public string CaddieCode { get; set; } = null!;
    public string CaddieName { get; set; } = null!;
    public string? Avatar { get; set; }
    public byte? Gender { get; set; }
    public string? GenderText { get; set; }
    public int ExperienceYear { get; set; }
    public int? HeightCm { get; set; }
    public decimal RatingAvg { get; set; }
    public int TotalBooking { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> VoiceRegions { get; set; } = new();
    public bool IsAvailable { get; set; }
}

public class MiniAppCaddieDetailDto
{
    public Guid Id { get; set; }
    public string CaddieCode { get; set; } = null!;
    public string CaddieName { get; set; } = null!;
    public string? Avatar { get; set; }
    public byte? Gender { get; set; }
    public string? GenderText { get; set; }
    public int ExperienceYear { get; set; }
    public int? HeightCm { get; set; }
    public decimal RatingAvg { get; set; }
    public int TotalBooking { get; set; }
    public List<string> Languages { get; set; } = new();
    public List<string> VoiceRegions { get; set; } = new();
    public List<MiniAppCaddieReviewDto> RecentReviews { get; set; } = new();
}

public class MiniAppCaddieReviewDto
{
    public int OverallRating { get; set; }
    public string? Comment { get; set; }
    public string? CustomerName { get; set; }
    public DateTime CreationTime { get; set; }
    public List<CaddieRatingDetailDto> Details { get; set; } = new();
}

public class MiniAppCreateCaddieBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid CaddieId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int? NumberOfHoles { get; set; }
    public string? Note { get; set; }
}

public class MiniAppCreateCaddieRatingDto
{
    public Guid CustomerId { get; set; }
    public Guid BookingId { get; set; }
    public int OverallRating { get; set; }
    public string? Comment { get; set; }
    public List<MiniAppSkillRatingDto> SkillRatings { get; set; } = new();
}

public class MiniAppSkillRatingDto
{
    public Guid SkillId { get; set; }
    public int Score { get; set; }
}

public class MiniAppCaddieBookingHistoryDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = null!;
    public string CaddieName { get; set; } = null!;
    public string? CaddieAvatar { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int? NumberOfHoles { get; set; }
    public byte Status { get; set; }
    public string? StatusText { get; set; }
    public byte PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public bool HasRating { get; set; }
}

// ── Response wrappers (ZaloBaseResponse pattern) ──────────────────────

/// <summary>GET /api/mini-app/caddie/available</summary>
public class MiniAppCaddieListResponse : ZaloBaseResponse
{
    public List<MiniAppCaddieListDto>? Data { get; set; }
}

/// <summary>GET /api/mini-app/caddie/{id}</summary>
public class MiniAppCaddieDetailResponse : ZaloBaseResponse
{
    public MiniAppCaddieDetailDto? Data { get; set; }
}

/// <summary>POST /api/mini-app/caddie/booking</summary>
public class MiniAppCaddieBookingResponse : ZaloBaseResponse
{
    public MiniAppCaddieBookingHistoryDto? Data { get; set; }
}

/// <summary>GET /api/mini-app/caddie/booking/history</summary>
public class MiniAppCaddieBookingHistoryResponse : ZaloBaseResponse
{
    public List<MiniAppCaddieBookingHistoryDto>? Data { get; set; }
}

/// <summary>POST /api/mini-app/caddie/rating</summary>
public class MiniAppCaddieRatingResponse : ZaloBaseResponse
{
    public object? Data { get; set; }
}

/// <summary>GET /api/mini-app/caddie/skills</summary>
public class MiniAppCaddieSkillsResponse : ZaloBaseResponse
{
    public List<CaddieSkillDto>? Data { get; set; }
}
