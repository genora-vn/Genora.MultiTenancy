using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class GetMiniAppCalendarListInput : PagedAndSortedResultRequestDto
    {
        public Guid? CustomerId { get; set; }
        [Required(ErrorMessage ="Vui lòng nhập mã sân")]
        public string GolfCourseCode { get; set; }
        public DateTime? Date { get; set; }
        public int? FrameTime { get; set; }
        public string? PromotionType { get; set; }
    }
}
