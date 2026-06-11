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
    public decimal OverallRating { get; set; }
    public string? Comment { get; set; }
    public string? CustomerName { get; set; }
    public DateTime CreationTime { get; set; }
    public List<CaddieRatingDetailDto> Details { get; set; } = new();
}

public class MiniAppCreateCaddieBookingDto
{
    public Guid CustomerId { get; set; }
    /// <summary>Danh sách caddie cần book (1 hoặc nhiều)</summary>
    public List<MiniAppBookingCaddieItemDto> Caddies { get; set; } = new();
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int? NumberOfHoles { get; set; }
    /// <summary>Tổng phí dịch vụ Caddie (VNĐ)</summary>
    public decimal TotalCaddieFee { get; set; }
    /// <summary>Phương thức thanh toán: COD = 0, Online = 1, BankTransfer = 2</summary>
    public byte PaymentMethod { get; set; }
    public string? Note { get; set; }
}

public class MiniAppBookingCaddieItemDto
{
    public Guid CaddieId { get; set; }
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
    public string? CaddieCode { get; set; }
    public string? CaddieAvatar { get; set; }
    public decimal CaddieRatingAvg { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int? NumberOfHoles { get; set; }
    public byte Status { get; set; }
    public string? StatusText { get; set; }
    public byte PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public decimal TotalCaddieFee { get; set; }
    public byte PaymentMethod { get; set; }
    public bool HasRating { get; set; }
}

/// <summary>Chi tiết lịch đặt caddie cho Mini App</summary>
public class MiniAppCaddieBookingDetailDto
{
    // Thông tin đặt lịch
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = null!;
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int? NumberOfHoles { get; set; }
    public byte Status { get; set; }
    public string? StatusText { get; set; }
    public byte PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public decimal TotalCaddieFee { get; set; }
    public byte PaymentMethod { get; set; }
    public string? PaymentMethodText { get; set; }
    public byte CheckinStatus { get; set; }
    public string? CheckinStatusText { get; set; }
    public DateTime? CheckinTime { get; set; }
    public string? Note { get; set; }
    public string? CancelReason { get; set; }
    public DateTime CreationTime { get; set; }

    // Thông tin khách hàng
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? CustomerPhone { get; set; }

    // Thông tin sân golf
    public Guid GolfCourseId { get; set; }
    public string? GolfCourseName { get; set; }
    public string? GolfCourseAddress { get; set; }

    // Danh sách caddie trong booking
    public List<MiniAppBookingCaddieDetailDto> Caddies { get; set; } = new();
}

public class MiniAppBookingCaddieDetailDto
{
    public Guid CaddieId { get; set; }
    public string CaddieName { get; set; } = null!;
    public string? CaddieCode { get; set; }
    public string? CaddieAvatar { get; set; }
    public decimal RatingAvg { get; set; }
    public string? Phone { get; set; }
    public byte? Gender { get; set; }
    public string? GenderText { get; set; }
    public string? Note { get; set; }
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

/// <summary>GET /api/mini-app/caddie/booking/{id}</summary>
public class MiniAppCaddieBookingDetailResponse : ZaloBaseResponse
{
    public MiniAppCaddieBookingDetailDto? Data { get; set; }
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

/// <summary>GET /api/mini-app/caddie/languages</summary>
public class MiniAppCaddieLanguagesResponse : ZaloBaseResponse
{
    public List<MiniAppLanguageDto>? Data { get; set; }
}

public class MiniAppLanguageDto
{
    public Guid Id { get; set; }
    public string LanguageCode { get; set; } = null!;
    public string LanguageName { get; set; } = null!;
    public string? NativeName { get; set; }
    public int SortOrder { get; set; }
}
