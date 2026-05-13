using Genora.MultiTenancy.Enums;
using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyBookingListDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public string? CustomerAvatar { get; set; }
    public Guid StylistId { get; set; }
    public string? StylistName { get; set; }
    public string? ServicesSummary { get; set; }
    public int ServiceCount { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public string? StatusText { get; set; }
    public SalonBeautyPaymentStatus PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public SalonBeautyCheckinStatus CheckinStatus { get; set; }
}