using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Genora.MultiTenancy.Enums;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;

public interface ISalonBeautyBookingAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyBookingListDto>> GetListAsync(GetSalonBeautyBookingListInput input);
    Task<SalonBeautyBookingDetailDto> GetAsync(Guid id);
    Task<SalonBeautyBookingDetailDto> CreateAsync(CreateSalonBeautyBookingDto input);
    Task<SalonBeautyBookingDetailDto> UpdateAsync(Guid id, UpdateSalonBeautyBookingDto input);
    Task<SalonBeautyBookingDetailDto> UpdateStatusAsync(Guid id, UpdateBookingStatusDto input);
    Task<SalonBeautyBookingDetailDto> CheckinAsync(Guid id);
    Task<SalonBeautyBookingDetailDto> UpdatePaymentAsync(Guid id, UpdateBookingPaymentDto input);
    Task<SalonBeautyBookingDetailDto> CancelAsync(Guid id, CancelBookingDto input);
    Task DeleteAsync(Guid id);
    Task<List<SalonBeautyBookingCalendarDto>> GetCalendarEventsAsync(DateTime from, DateTime to, Guid? stylistId = null, Guid? serviceId = null);
    Task<BookingStatisticsDto> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);
    Task<List<SalonBeautyCustomerLookupDto>> GetCustomerLookupAsync(string? filter);
    Task<List<SalonBeautyServiceLookupDto>> GetServiceLookupAsync();
    Task<List<SalonBeautyStylistLookupDto>> GetStylistLookupAsync();
}

public class CreateSalonBeautyBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid StylistId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    public decimal? Surcharge { get; set; }
    public decimal? Discount { get; set; }
    public List<CreateSalonBeautyBookingItemDto> Items { get; set; } = new();
}

public class UpdateSalonBeautyBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid StylistId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan? EndTime { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public string? CustomerNote { get; set; }
    public string? InternalNote { get; set; }
    public decimal? Surcharge { get; set; }
    public decimal? Discount { get; set; }
    public List<CreateSalonBeautyBookingItemDto> Items { get; set; } = new();
}

public class CreateSalonBeautyBookingItemDto
{
    public Guid ServiceId { get; set; }
    public Guid? StylistId { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
}

public class UpdateBookingStatusDto
{
    public SalonBeautyBookingStatus Status { get; set; }
}

public class UpdateBookingPaymentDto
{
    public SalonBeautyPaymentStatus PaymentStatus { get; set; }
    public SalonBeautyPaymentMethod? PaymentMethod { get; set; }
}

public class CancelBookingDto
{
    public SalonBeautyCancelReason CancelReason { get; set; }
    public string? CancelNote { get; set; }
}

public class GetSalonBeautyBookingListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? StylistId { get; set; }
    public byte? Status { get; set; }
    public byte? PaymentStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

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

public class SalonBeautyBookingDetailDto
{
    public Guid Id { get; set; }
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

public class SalonBeautyBookingItemDto
{
    public Guid Id { get; set; }
    public Guid ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? ServiceCategoryName { get; set; }
    public Guid? StylistId { get; set; }
    public string? StylistName { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
}

public class SalonBeautyBookingActivityDto
{
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime Time { get; set; }
    public bool IsDanger { get; set; }
}

public class SalonBeautyBookingCalendarDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string CustomerName { get; set; } = null!;
    public string? CustomerPhone { get; set; }
    public Guid StylistId { get; set; }
    public string StylistName { get; set; } = null!;
    public string? ServiceName { get; set; }
    public int ServiceCount { get; set; }
    public DateTime Start { get; set; }
    public DateTime End { get; set; }
    public string Status { get; set; } = null!;
    public string StatusText { get; set; } = null!;
    public string StatusColor { get; set; } = null!;
    public decimal TotalAmount { get; set; }
}

public class BookingStatisticsDto
{
    public int TotalBookings { get; set; }
    public decimal TotalBookingsChangePercent { get; set; }
    public decimal TotalValue { get; set; }
    public decimal TotalValueChangePercent { get; set; }
    public decimal CompletionRate { get; set; }
    public string CompletionTrendText { get; set; } = "Ổn định";
    public int PendingCount { get; set; }
    public int ConfirmedCount { get; set; }
    public int ProcessingCount { get; set; }
    public int CompletedCount { get; set; }
    public int CancelledCount { get; set; }
    public int NewUnprocessedCount { get; set; }
}

public class SalonBeautyCustomerLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Phone { get; set; }
    public string? Avatar { get; set; }
    public string? Code { get; set; }
}

public class SalonBeautyServiceLookupDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? CategoryName { get; set; }
    public decimal Price { get; set; }
    public int Duration { get; set; }
}

public class SalonBeautyStylistLookupDto
{
    public Guid Id { get; set; }
    public string DisplayName { get; set; } = null!;
    public string? Avatar { get; set; }
    public string? RoleText { get; set; }
    public byte? Role { get; set; }
}
