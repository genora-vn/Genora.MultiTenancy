using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;

public class CalendarSlotDeleteDto
{
    public Guid GolfCourseId { get; set; }

    public DateTime ApplyDateFrom { get; set; }

    public DateTime ApplyDateTo { get; set; }

    public TimeSpan TimeFrom { get; set; }

    public TimeSpan TimeTo { get; set; }
}

public class CalendarSlotDeleteResultDto
{
    public int TotalSlotsFound { get; set; }

    public int DeletedSlots { get; set; }

    public int DeletedPrices { get; set; }

    public int SkippedSlots { get; set; }

    public string? ErrorMessage { get; set; }

    public bool Success => string.IsNullOrEmpty(ErrorMessage);
}
