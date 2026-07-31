using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppBookings
{
    public class MiniAppBookingListDto : ZaloBaseResponse
    {
        public PagedResultDto<BookingListData>? Data { get; set; }
    }
    public class BookingListData
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
        public decimal TotalAmount { get; set; }
        public string? FrameTimes { get; set; }
        public int? NumberHoles { get; set; }
        public PaymentMethod? PaymentMethod { get; set; }
        public BookingStatus Status { get; set; }
        public BookingSource Source { get; set; }
        public string VNDayOfWeek { get; set; }
        public bool IsCancellationPolicy { get; set; }

        /// <summary>Tổng phí thuê Caddie (nếu booking có đặt Caddie). Null nếu không dùng module Caddie.</summary>
        public decimal? TotalCaddieFee { get; set; }

        /// <summary>Danh sách Caddie đã book kèm booking golf (lấy từ AppBookingPlayers). Rỗng nếu không có.</summary>
        public List<MiniAppBookingGolfCaddieDto> Caddies { get; set; } = new();

        // ── Các field bổ sung để phục vụ chỉnh sửa booking (giống API detail) ──
        /// <summary>Danh sách người chơi chi tiết (kèm CaddieId/CaddieName/AppCaddieBookingDetailId).</summary>
        public List<AppBookingPlayerDto> Players { get; set; } = new();
        public List<int>? Utilities { get; set; }
        public bool IsExportInvoice { get; set; }
        public string? CompanyName { get; set; }
        public string? TaxCode { get; set; }
        public string? CompanyAddress { get; set; }
        public string? InvoiceEmail { get; set; }
    }

    /// <summary>
    /// Caddie đã gán cho người chơi trong booking golf (đọc từ AppBookingPlayers).
    /// Dùng chung cho cả list và detail booking golf mini app.
    /// </summary>
    public class MiniAppBookingGolfCaddieDto
    {
        /// <summary>Id booking Caddie (AppCaddieBooking) — để mini app gọi API sửa/hủy Caddie</summary>
        public Guid? CaddieBookingId { get; set; }
        public Guid? CaddieId { get; set; }
        public string? CaddieName { get; set; }
        /// <summary>Tên người chơi được gán Caddie này</summary>
        public string? PlayerName { get; set; }
    }
}
