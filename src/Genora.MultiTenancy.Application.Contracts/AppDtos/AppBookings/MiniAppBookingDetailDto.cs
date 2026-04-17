
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppBookings
{
    public class MiniAppBookingDetailDto : ZaloBaseResponse
    {
        public BookingDetailData? Data { get; set; }
    }
    public class BookingDetailData
    {
        public Guid Id { get; set; }
        public Guid? TenantId { get; set; }
        public string BookingCode { get; set; }
        public Guid CustomerId { get; set; }
        public Guid GolfCourseId { get; set; }
        public Guid? CalendarSlotId { get; set; }
        public DateTime PlayDate { get; set; }
        public DateTime CreationTime { get; set; }
        public int NumberOfGolfers { get; set; }
        public int MaxSlots { get; set; }

        public decimal? PricePerGolfer { get; set; }
        public decimal OriginalTotalAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string? FrameTimes { get; set; }
        public int? NumberHoles { get; set; }
        public List<int>? Utilities { get; set; }
        public bool IsExportInvoice { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public BookingStatus Status { get; set; }
        public BookingSource Source { get; set; }
        public string VNDayOfWeek { get; set; }
        public List<AppBookingPlayerDto> Players { get; set; } = new();

        public string? CompanyName { get; set; }
        public string? TaxCode { get; set; }
        public string? CompanyAddress { get; set; }
        public string? InvoiceEmail { get; set; }

        public string? CustomerTypeCode { get; set; }
        public bool IsMemberSupported { get; set; }
        public int? MaxMemberGuest { get; set; }
        public decimal? MemberGuestPrice { get; set; }
        public decimal VisitorPrice { get; set; }

        /// <summary>
        /// Tổng tiền khách hàng phải trả (theo giá ưu đãi) dựa trên numberOfGolfers và loại khách hàng.
        /// </summary>
        public decimal CustomerBillTotalPrice { get; set; }

        /// <summary>
        /// Tổng tiền theo giá gốc (OriginalPrice trong AppCustomerTypes).
        /// </summary>
        public decimal OriginalBillTotalPrice { get; set; }

        /// <summary>
        /// Tổng tiền được chiết khấu = OriginalBillTotalPrice - CustomerBillTotalPrice.
        /// </summary>
        public decimal DiscountTotalPrice { get; set; }
    }
}
