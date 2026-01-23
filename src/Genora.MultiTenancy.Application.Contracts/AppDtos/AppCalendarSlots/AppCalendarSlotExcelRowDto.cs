
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class AppCalendarSlotExcelRowDto
    {
        public string? GolfCourseName { get; set; }
        public string? GolfCourseCode { get; set; }

        // NEW: "Trong tuần" | "Cuối tuần" | "Ngày lễ"
        public string? DayType { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxSlots { get; set; }
        public string PromotionType { get; set; }
        public int Gap { get; set; }
        public string InternalNote { get; set; }
        public List<CustomerTypeExcelRowDto> CustomerTypePrice { get; set; } = new List<CustomerTypeExcelRowDto>();
    }
    public class CustomerTypeExcelRowDto
    {
        public string CustomerType { get; set; } = default!;
        public decimal? Price9 { get; set; }
        public decimal Price18 { get; set; }
        public decimal? Price27 { get; set; }
        public decimal? Price36 { get; set; }
    }
}
