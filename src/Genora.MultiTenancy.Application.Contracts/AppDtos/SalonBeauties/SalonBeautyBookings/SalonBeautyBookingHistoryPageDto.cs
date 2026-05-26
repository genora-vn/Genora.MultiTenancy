using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyBookings;

public class GetSalonBeautyBookingHistoryInput : PagedAndSortedResultRequestDto
{
    public Guid BookingId { get; set; }
    public string? ActionType { get; set; }
}

public class SalonBeautyBookingHistoryPageDto
{
    public Guid BookingId { get; set; }
    public string BookingCode { get; set; } = default!;
    public string? CustomerName { get; set; }
    public string? CustomerPhoneMasked { get; set; }
    public SalonBeautyBookingStatus Status { get; set; }
    public string StatusText { get; set; } = default!;
    public DateTime CreationTime { get; set; }
    public DateTime? LastActivityTime { get; set; }
    public int TotalActions { get; set; }

    public List<SalonBeautyBookingHistoryActionTypeOptionDto> ActionTypeOptions { get; set; } = new();
    public PagedResultDto<SalonBeautyBookingHistoryItemDto> PagedActivities { get; set; }
        = new PagedResultDto<SalonBeautyBookingHistoryItemDto>(0, new List<SalonBeautyBookingHistoryItemDto>());
}

public class SalonBeautyBookingHistoryItemDto
{
    public DateTime Time { get; set; }
    public string PerformedBy { get; set; } = default!;
    public string ActionType { get; set; } = default!;
    public string ActionTypeText { get; set; } = default!;
    public string ActionTypeClass { get; set; } = default!;
    public string Title { get; set; } = default!;
    public string Description { get; set; } = default!;
    public bool IsDanger { get; set; }
}

public class SalonBeautyBookingHistoryActionTypeOptionDto
{
    public string Value { get; set; } = default!;
    public string Text { get; set; } = default!;
}
