using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using Genora.MultiTenancy.Enums;
using System.Threading.Tasks;
using System;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;

public interface ISalonBeautyBookingAppService : IApplicationService
{
    Task<PagedResultDto<SalonBeautyBookingListDto>> GetListAsync(GetSalonBeautyBookingListInput input);
    Task<SalonBeautyBookingDetailDto> GetAsync(Guid id);
    Task<SalonBeautyBookingDetailDto> CreateAsync(CreateSalonBeautyBookingDto input);
    Task<SalonBeautyBookingDetailDto> UpdateAsync(Guid id, UpdateSalonBeautyBookingDto input);
    Task<SalonBeautyBookingDetailDto> CheckinAsync(Guid id);
    Task<SalonBeautyBookingDetailDto> UpdatePaymentAsync(Guid id, UpdateBookingPaymentDto input);
    Task<SalonBeautyBookingDetailDto> CancelAsync(Guid id, CancelBookingDto input);
    Task DeleteAsync(Guid id);
}

public class CreateSalonBeautyBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid StylistId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Note { get; set; }
}

public class UpdateSalonBeautyBookingDto
{
    public Guid CustomerId { get; set; }
    public Guid ServiceId { get; set; }
    public Guid StylistId { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public string? Note { get; set; }
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
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class SalonBeautyBookingListDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public Guid StylistId { get; set; }
    public string? StylistName { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public SalonBeautyPaymentStatus PaymentStatus { get; set; }
    public SalonBeautyCheckinStatus CheckinStatus { get; set; }
}

public class SalonBeautyBookingDetailDto
{
    public Guid Id { get; set; }
    public string BookingCode { get; set; } = null!;
    public Guid CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid ServiceId { get; set; }
    public string? ServiceName { get; set; }
    public Guid StylistId { get; set; }
    public string? StylistName { get; set; }
    public DateTime BookingDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public decimal TotalAmount { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public SalonBeautyPaymentStatus PaymentStatus { get; set; }
    public SalonBeautyPaymentMethod? PaymentMethod { get; set; }
    public SalonBeautyCheckinStatus CheckinStatus { get; set; }
    public DateTime? CheckinTime { get; set; }
    public string? Note { get; set; }
    public SalonBeautyCancelReason? CancelReason { get; set; }
    public string? CancelNote { get; set; }
}
