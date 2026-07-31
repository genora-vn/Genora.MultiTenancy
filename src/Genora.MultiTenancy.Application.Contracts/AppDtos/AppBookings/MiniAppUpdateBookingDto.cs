using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppBookings;
public class MiniAppUpdateBookingDto
{
    [Required]
    public Guid CustomerId { get; set; }

    public DateTime PlayDate { get; set; }

    [Required]
    public Guid CalendarSlotId { get; set; }

    [Range(1, int.MaxValue)]
    public int NumberOfGolfers { get; set; }

    public List<CreateUpdateBookingPlayerDto>? Players { get; set; } = new();

    public decimal? PricePerGolfer { get; set; }

    /// <summary>Tổng phí thuê Caddie đi kèm (VNĐ, được phép null). Đã cộng vào TotalAmount.</summary>
    public decimal? TotalCaddieFee { get; set; }

    /// <summary>
    /// [UNIFIED FLOW - tùy chọn] Danh sách Caddie sau khi sửa (thêm/bớt). Khi có giá trị → server reconcile:
    /// tạo/tái dùng AppCaddieBooking, thêm detail Caddie mới, gỡ detail bị bỏ, tính lại TotalCaddieFee
    /// (= số caddie × GolfCourse.CaddieFee), cập nhật AppBookings + gán CaddieId/CaddieName vào đúng người chơi.
    /// Khi null → giữ nguyên logic cũ (mini app khác + luồng đặt Caddie riêng KHÔNG bị ảnh hưởng).
    /// </summary>
    public List<MiniAppInlineCaddieInput>? CaddieAssignments { get; set; }

    public List<int>? Utilities { get; set; } = new();

    public short? NumberHoles { get; set; } = 18;

    public bool IsExportInvoice { get; set; }

    public string? CompanyName { get; set; }
    public string? TaxCode { get; set; }
    public string? CompanyAddress { get; set; }
    public string? InvoiceEmail { get; set; }
}