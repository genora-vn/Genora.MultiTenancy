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