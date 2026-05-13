using Volo.Abp.Application.Services;
using Volo.Abp.Application.Dtos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;

public interface ISalonBeautyBookingAppService :
    ICrudAppService<
        SalonBeautyBookingDetailDto,
        Guid,
        GetSalonBeautyBookingListInput,
        CreateSalonBeautyBookingDto,
        UpdateSalonBeautyBookingDto>
{
    Task<SalonBeautyBookingDetailDto> UpdateStatusAsync(Guid id, UpdateBookingStatusDto input);
    Task<SalonBeautyBookingDetailDto> CheckinAsync(Guid id);
    Task<SalonBeautyBookingDetailDto> UpdatePaymentAsync(Guid id, UpdateBookingPaymentDto input);
    Task<SalonBeautyBookingDetailDto> CancelAsync(Guid id, CancelBookingDto input);
    Task<List<SalonBeautyBookingCalendarDto>> GetCalendarEventsAsync(DateTime from, DateTime to, Guid? stylistId = null, Guid? serviceId = null);
    Task<BookingStatisticsDto> GetStatisticsAsync(DateTime? fromDate, DateTime? toDate);
    Task<List<SalonBeautyCustomerLookupDto>> GetCustomerLookupAsync(string? filter);
    Task<List<SalonBeautyServiceLookupDto>> GetServiceLookupAsync();
    Task<List<SalonBeautyStylistLookupDto>> GetStylistLookupAsync();
}
