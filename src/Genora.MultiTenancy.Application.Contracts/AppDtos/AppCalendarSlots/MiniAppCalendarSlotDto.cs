using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots
{
    public class MiniAppCalendarSlotDto : ZaloBaseResponse
    {
        public PagedResultDto<CalendarSlotData> Data { get; set; }
        public List<FrameTimeOfDay> FrameTimeOfDays { get; set; }
    }
    public class CalendarSlotData
    {
        public Guid Id { get; set; }
        public string? GolfCourseCode { get; set; }
        public string? FrameTime { get; set; }
        public int FrameTimeOfDayId { get; set; }
        public string? FrameTimeOfDayName { get; set; }
        public DateTime? PlayDate { get; set; }
        public TimeSpan? TimeFrom { get; set; }
        public TimeSpan? TimeTo { get; set; }
        public int MaxSlots { get; set; }
        public int PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public decimal CustomerTypePrice { get; set; }
        public decimal VisitorPrice { get; set; } 
        public decimal DiscountPercent { get; set; }
        public bool IsBestDeal { get; set; }
    }
    public class FrameTimeOfDay
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}



