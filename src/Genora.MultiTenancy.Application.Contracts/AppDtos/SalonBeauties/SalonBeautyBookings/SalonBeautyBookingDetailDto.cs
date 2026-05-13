using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
public class SalonBeautyBookingDetailDto : EntityDto<Guid>
{
    public string BookingCode { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public string? CustomerAvatar { get; set; }
    public string? CustomerCode { get; set; }
    public int CustomerLoyaltyPoint { get; set; }
    public Guid StylistId { get; set; }
    public string? StylistName { get; set; }
    public string? StylistAvatar { get; set; }
    public string? StylistRoleText { get; set; }
    public string? ServicesSummary { get; set; }
    public int ServiceCount { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal SubTotal { get; set; }
    public decimal Surcharge { get; set; }
    public decimal Discount { get; set; }
    public decimal TotalAmount { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public string? StatusText { get; set; }
    public SalonBeautyPaymentStatus PaymentStatus { get; set; }
    public string? PaymentStatusText { get; set; }
    public SalonBeautyPaymentMethod? PaymentMethod { get; set; }
    public string? PaymentMethodText { get; set; }
    public SalonBeautyCheckinStatus CheckinStatus { get; set; }
    public string? CheckinStatusText { get; set; }
    public DateTime? CheckinTime { get; set; }
    public DateTime? PaidTime { get; set; }
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    public SalonBeautyCancelReason? CancelReason { get; set; }
    public string? CancelNote { get; set; }
    public DateTime CreationTime { get; set; }
    public DateTime? LastModificationTime { get; set; }
    public List<SalonBeautyBookingItemDto> Items { get; set; } = new();
    public List<SalonBeautyBookingActivityDto> Activities { get; set; } = new();
}