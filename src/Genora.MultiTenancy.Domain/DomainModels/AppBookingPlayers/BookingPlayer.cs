using Genora.MultiTenancy.DomainModels.AppBookings;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppBookingPlayers;

[Table("AppBookingPlayers")]
public class BookingPlayer : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid BookingId { get; set; }
    public virtual Booking Booking { get; set; } = null!;

    public Guid? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    [Required]
    [StringLength(150)]
    public string PlayerName { get; set; } = null!;

    public decimal? PricePerPlayer { get; set; }

    [StringLength(50)]
    public string? VgaCode { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }

    /// <summary>
    /// Caddie được đặt cho người chơi này (soft reference tới AppCaddies, không FK để tránh ràng buộc cross-tenant).
    /// Mini App gọi API đặt Caddie trước, sau đó truyền CaddieId vào từng người chơi khi tạo booking golf.
    /// </summary>
    public Guid? CaddieId { get; set; }

    /// <summary>
    /// Trỏ về AppCaddieBooking (HEADER) — liên kết booking golf với booking Caddie.
    /// LƯU Ý: đây là Id của bảng AppCaddieBookings (không phải AppCaddieBookingDetails).
    /// </summary>
    public Guid? CaddieBookingId { get; set; }

    /// <summary>
    /// Trỏ về AppCaddieBookingDetail (DÒNG chi tiết Caddie cụ thể gán cho người chơi này).
    /// Là Id của bảng AppCaddieBookingDetails — dùng để cập nhật/gỡ đúng dòng khi đổi/hủy Caddie.
    /// </summary>
    public Guid? AppCaddieBookingDetailId { get; set; }

    /// <summary>
    /// Tên Caddie (denormalize để tiện hiển thị, tránh join sang AppCaddies).
    /// </summary>
    [StringLength(255)]
    public string? CaddieName { get; set; }

    protected BookingPlayer() { }

    public BookingPlayer(Guid id, Guid bookingId, Guid? customerId, string playerName, decimal? pricePerPlayer, string? vgaCode, string notes = "") : base(id)
    {
        BookingId = bookingId;
        CustomerId = customerId;
        PlayerName = playerName;
        PricePerPlayer = pricePerPlayer;
        VgaCode = vgaCode;
        Notes = notes;
    }
}