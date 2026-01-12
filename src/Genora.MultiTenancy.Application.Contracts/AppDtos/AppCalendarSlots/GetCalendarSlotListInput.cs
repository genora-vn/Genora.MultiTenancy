using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;

public class GetCalendarSlotListInput : PagedAndSortedResultRequestDto
{
    public Guid? GolfCourseId { get; set; }

    public DateTime? ApplyDateFrom { get; set; }
    public DateTime? ApplyDateTo { get; set; }

    public Guid? PromotionType { get; set; }
    public bool? IsActive { get; set; }
}