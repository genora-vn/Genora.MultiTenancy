using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyTimeSlots;

public interface ISalonBeautyTimeSlotAppService : IApplicationService
{
    /// <summary>
    /// Danh sách lịch làm việc, group theo stylist.
    /// </summary>
    Task<PagedResultDto<SalonBeautyTimeSlotGroupedDto>> GetListAsync(GetSalonBeautyTimeSlotListInput input);

    /// <summary>
    /// Lấy chi tiết schedule của 1 stylist (dải ngày + giờ + ranges).
    /// </summary>
    Task<SalonBeautyTimeSlotEditDto> GetByStylistAsync(Guid stylistId, DateTime? fromDate = null, DateTime? toDate = null);

    /// <summary>
    /// Lấy danh sách events cho calendar view.
    /// </summary>
    Task<List<SalonBeautyTimeSlotDto>> GetCalendarEventsAsync(GetSalonBeautyTimeSlotCalendarInput input);

    /// <summary>
    /// Auto-generate danh sách khung giờ trong ngày dựa vào config Location
    /// (open_time, close_time, slot_duration, buffer_time).
    /// Dùng để preview/auto-fill UI khi tạo time slot.
    /// </summary>
    Task<List<TimeRangeDto>> GenerateRangesByLocationAsync(Guid locationId);

    /// <summary>
    /// Lookup stylist - filter theo location, dùng cho cascade dropdown trên modal.
    /// </summary>
    Task<List<SalonBeautyStylistLookupDto>> GetStylistLookupAsync(Guid? locationId = null);

    Task<List<SalonBeautyTimeSlotDto>> CreateAsync(CreateSalonBeautyTimeSlotDto input);

    Task<List<SalonBeautyTimeSlotDto>> UpdateByStylistAsync(Guid stylistId, UpdateSalonBeautyTimeSlotDto input);

    /// <summary>
    /// Update status 1 slot trên calendar.
    /// </summary>
    Task<SalonBeautyTimeSlotDto> UpdateStatusAsync(Guid id, UpdateSalonBeautyTimeSlotStatusDto input);

    Task DeleteByStylistAsync(Guid stylistId);
}

public class SalonBeautyTimeSlotEditDto
{
    public Guid StylistId { get; set; }
    public string? StylistName { get; set; }
    public Guid? LocationId { get; set; }
    public string? LocationName { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public List<TimeRangeDto> Ranges { get; set; } = new();
    public int WeekdayMask { get; set; }
    public bool IsShowOnApp { get; set; }
    public byte Status { get; set; }
    public string? Note { get; set; }
}
