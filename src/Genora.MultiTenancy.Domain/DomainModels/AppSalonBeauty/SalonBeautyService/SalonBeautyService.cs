using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyServices")]
public class SalonBeautyService : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    public Guid CategoryId { get; set; }

    public decimal Price { get; set; }

    public int Duration { get; set; }

    public byte? ApplicableRole { get; set; }

    public byte? ApplicableLevel { get; set; }

    public byte Status { get; set; } = 1;

    public bool IsShowOnApp { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public int SortOrder { get; set; }

    public virtual SalonBeautyServiceCategory? Category { get; set; }
    public virtual ICollection<SalonBeautyBooking> Bookings { get; set; } = new List<SalonBeautyBooking>();
    public virtual ICollection<SalonBeautyBookingService> BookingServices { get; set; } = new List<SalonBeautyBookingService>();
}
