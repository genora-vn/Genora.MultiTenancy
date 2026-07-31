using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class CaddieBookingDto : EntityDto<Guid>
{
    public string BookingCode { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string? PhoneMasked { get; set; }
    public Guid GolfCourseId { get; set; }
    public string? GolfCourseName { get; set; }
    public Guid CaddieId { get; set; }
    public string? CaddieName { get; set; }
    public string? CaddieCode { get; set; }
    /// <summary>Tên tất cả Caddie trong booking, nối chuỗi. Ví dụ: "Nguyễn Bảo Nam, Đàm Thùy Trang".</summary>
    public string? CaddieNames { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public int? NumberOfHoles { get; set; }
    public string? Note { get; set; }
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
    public string? CancelReason { get; set; }
    public DateTime CreationTime { get; set; }
    /// <summary>Average rating for this booking (from CaddieRating details). Null if not rated.</summary>
    public decimal? BookingRatingAvg { get; set; }
}

public class GetCaddieBookingListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? CaddieId { get; set; }
    public Guid? GolfCourseId { get; set; }
    public byte? Status { get; set; }
    public byte? PaymentStatus { get; set; }
    public byte? CheckinStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class UpdateCaddieBookingStatusDto
{
    public byte? Status { get; set; }
    public byte? PaymentStatus { get; set; }
    public byte? CheckinStatus { get; set; }
    public string? CancelReason { get; set; }
}
