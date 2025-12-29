
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class AppCalendarSlotExcelRowDto
    {
        public string? GolfCourseName { get; set; }
        public string? GolfCourseCode { get; set; }
        public DateTime PlayDate { get; set; }
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxSlots { get; set; }
        public string PromotionType { get; set; }
        public string InternalNote { get; set; }
        public List<CustomerTypeExcelRowDto> CustomerTypePrice { get; set; } = new List<CustomerTypeExcelRowDto>();
    }
    public class CustomerTypeExcelRowDto
    {
        public string CustomerType { get; set; }
        public decimal? Price { get; set; }
    }
}
