using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class CaddieScheduleDto : EntityDto<Guid>
{
    public Guid CaddieId { get; set; }
    public string? CaddieName { get; set; }
    public string? CaddieCode { get; set; }
    public DateTime WorkDate { get; set; }
    public byte ShiftCode { get; set; }
    public string? ShiftCodeText { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public byte SlotStatus { get; set; }
    public string? SlotStatusText { get; set; }
    public Guid? BookingId { get; set; }
    public string? BookingCode { get; set; }
    public bool IsNightShift { get; set; }
    public string? Note { get; set; }
}

public class CreateUpdateCaddieScheduleDto
{
    public Guid CaddieId { get; set; }
    public DateTime WorkDate { get; set; }
    public byte ShiftCode { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public byte SlotStatus { get; set; } = 1;
    public bool IsNightShift { get; set; }
    public string? Note { get; set; }
}

public class GetCaddieScheduleListInput : PagedAndSortedResultRequestDto
{
    public Guid? CaddieId { get; set; }
    public Guid? GolfCourseId { get; set; }
    public byte? SlotStatus { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
}

public class CaddieScheduleCalendarDto
{
    public DateTime WeekStart { get; set; }
    public DateTime WeekEnd { get; set; }
    public List<CaddieScheduleDto> Schedules { get; set; } = new();
}

public class DeleteCaddieScheduleRangeInput
{
    public DateTime FromDate { get; set; }
    public DateTime ToDate { get; set; }
    public TimeSpan? FromTime { get; set; }
    public TimeSpan? ToTime { get; set; }
    public Guid? CaddieId { get; set; }
}

public class DeleteCaddieScheduleRangeResultDto
{
    public int TotalFound { get; set; }
    public int DeletedCount { get; set; }
    public int SkippedCount { get; set; }
}
