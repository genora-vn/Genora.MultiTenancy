using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class GetMiniAppCalendarListInput : PagedAndSortedResultRequestDto
    {
        public Guid? CustomerId { get; set; }
        public string GolfCourseCode { get; set; }
        public DateTime? Date { get; set; }
        public int? FrameTime { get; set; }
        public int? PromotionType { get; set; }
    }
}
