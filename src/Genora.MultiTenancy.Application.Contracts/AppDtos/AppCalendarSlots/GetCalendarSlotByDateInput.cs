using System;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;
public class GetCalendarSlotByDateInput
{
    public Guid GolfCourseId { get; set; }
    public DateTime? ApplyDateFrom { get; set; }
    public DateTime? ApplyDateTo { get; set; }
}