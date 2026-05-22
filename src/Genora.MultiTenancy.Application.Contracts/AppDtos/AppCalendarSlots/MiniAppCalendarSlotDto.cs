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

        /// <summary>
        /// Cho phép hiển thị hình thức Thanh toán tại quầy trên Mini App.
        /// Đọc từ ABP Setting Genora.Payment.IsPayAtCounterEnabled (default = true).
        /// </summary>
        public bool IsPayAtCounterEnabled { get; set; } = true;

        /// <summary>
        /// Cho phép hiển thị hình thức Thanh toán chuyển khoản trên Mini App.
        /// Đọc từ ABP Setting Genora.Payment.IsPayBankTransferEnabled (default = true).
        /// </summary>
        public bool IsPayBankTransferEnabled { get; set; } = true;
    }
    public class CalendarSlotData
    {
        public Guid Id { get; set; }
        public Guid GolfCourseId { get; set; }
        public string? GolfCourseCode { get; set; }
        public string? FrameTime { get; set; }
        public int FrameTimeOfDayId { get; set; }
        public string? FrameTimeOfDayName { get; set; }
        public DateTime? PlayDate { get; set; }
        public TimeSpan? TimeFrom { get; set; }
        public TimeSpan? TimeTo { get; set; }
        public int MaxSlots { get; set; }

        /// <summary>
        /// Số chỗ còn trống. Frontend dùng để hiển thị cảnh báo:
        /// - slotAvailable = 0: "Tee-time đã đủ khách"
        /// - slotAvailable < numberOfGolfers: "Chỉ còn X chỗ trống"
        /// </summary>
        public int SlotAvailable { get; set; }

        public Guid PromotionId { get; set; }
        public string? PromotionName { get; set; }
        public decimal CustomerTypePrice { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal VisitorPrice { get; set; } 
        public decimal DiscountPercent { get; set; }
        public bool IsBestDeal { get; set; }

        public string? CustomerTypeCode { get; set; }
        public string? OriginalPriceSource { get; set; }

        public string? PromotionCode { get; set; }
        public string? PromotionIconUrl { get; set; }
        public string? PromotionColorCode { get; set; }

        /// <summary>Sân có áp dụng chính sách Member không.</summary>
        public bool IsMemberSupported { get; set; }

        /// <summary>Số Member Guest tối đa / 1 Member. Null khi IsMemberSupported = false.</summary>
        public int? MaxMemberGuest { get; set; }

        /// <summary>
        /// Giá Member Accompanied Guest (Code=MBG) theo số lỗ.
        /// Chỉ có giá trị khi IsMemberSupported=true VÀ khách hàng hiện tại là Member (Code=MB).
        /// </summary>
        public decimal? MemberGuestPrice { get; set; }

        /// <summary>
        /// Tổng tiền khách hàng phải trả (theo giá ưu đãi) dựa trên slotAvailable và loại khách hàng.
        /// - Visitor: visitorPrice * slotAvailable
        /// - Member (MB) + isMemberSupported=true: memberPrice + (memberGuestPrice * maxMemberGuest) + (max(0, slotAvailable - maxMemberGuest - 1) * visitorPrice)
        /// </summary>
        public decimal CustomerBillTotalPrice { get; set; }

        /// <summary>
        /// Tổng tiền theo giá gốc (OriginalPrice trong AppCustomerTypes) — cùng công thức với CustomerBillTotalPrice.
        /// </summary>
        public decimal OriginalBillTotalPrice { get; set; }

        /// <summary>
        /// Tổng tiền được chiết khấu = OriginalBillTotalPrice - CustomerBillTotalPrice.
        /// </summary>
        public decimal DiscountTotalPrice { get; set; }
    }
    public class FrameTimeOfDay
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
}



