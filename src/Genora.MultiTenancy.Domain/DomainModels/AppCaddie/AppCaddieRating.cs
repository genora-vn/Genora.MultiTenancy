using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieRatings")]
public class AppCaddieRating : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid BookingId { get; set; }

    public Guid CustomerId { get; set; }

    public Guid CaddieId { get; set; }

    public int OverallRating { get; set; }

    [StringLength(2000)]
    public string? Comment { get; set; }

    public byte ApprovalStatus { get; set; } = 1;

    public DateTime? ApprovedAt { get; set; }

    public Guid? ApprovedBy { get; set; }

    [StringLength(1000)]
    public string? RejectReason { get; set; }

    public virtual AppCaddieBooking? Booking { get; set; }
    public virtual AppCaddie? Caddie { get; set; }
    public virtual ICollection<AppCaddieRatingDetail> Details { get; set; } = new List<AppCaddieRatingDetail>();
}
