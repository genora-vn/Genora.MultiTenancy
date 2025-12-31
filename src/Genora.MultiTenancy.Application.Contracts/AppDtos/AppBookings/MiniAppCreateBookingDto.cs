using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppBookings;

public class MiniAppBookingPlayerInput
{
    public Guid? CustomerId { get; set; }  // nếu có
    [Required]
    [StringLength(200)]
    public string PlayerName { get; set; }
    [StringLength(500)]
    public string? Notes { get; set; }
    [StringLength(100)]
    public string? VgaCode { get; set; }
}

public class MiniAppCreateBookingDto
{
    /// <summary>Id khách hàng chính (người book)</summary>
    [Required]
    public Guid CustomerId { get; set; }

    /// <summary>Ngày chơi (date only)</summary>
    [Required]
    public DateTime PlayDate { get; set; }

    /// <summary>Sân golf</summary>
    [Required]
    public Guid GolfCourseId { get; set; }

    /// <summary>Id khung giờ (AppCalendarSlots)</summary>
    [Required]
    public Guid CalendarSlotId { get; set; }

    /// <summary>Số lượng golfer</summary>
    [Range(1, 100)]
    public int NumberOfGolfers { get; set; }

    /// <summary>Danh sách golfer chi tiết</summary>
    [Required]
    public List<MiniAppBookingPlayerInput> Players { get; set; } = new();

    /// <summary>Giá / 1 người</summary>
    [Range(0, double.MaxValue)]
    public decimal PricePerGolfer { get; set; }

    /// <summary>Tổng tiền</summary>
    [Range(0, double.MaxValue)]
    public decimal TotalAmount { get; set; }

    /// <summary>Phương thức thanh toán: 1: COD, 2: Online, 3: BankTransfer</summary>
    [Required]
    public PaymentMethod PaymentMethod { get; set; }

    /// <summary>Trạng thái booking (Mini App truyền mặc định 0 – Đang xử lý)</summary>
    [Required]
    public BookingStatus Status { get; set; } = BookingStatus.Processing;

    /// <summary>Nguồn: 1: MiniApp, 2: Hotline, 3: Agent (MiniApp mặc định = 1)</summary>
    [Required]
    public BookingSource Source { get; set; } = BookingSource.MiniApp;
    public List<int>? Utilities { get; set; }
    [Required]
    public short NumberHoles { get; set; }
    [Required]
    public bool IsExportInvoice { get; set; }
}
