using System;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;
public class GetCalendarSlotByDateInput
{
    public Guid GolfCourseId { get; set; }
    public DateTime ApplyDate { get; set; }
}