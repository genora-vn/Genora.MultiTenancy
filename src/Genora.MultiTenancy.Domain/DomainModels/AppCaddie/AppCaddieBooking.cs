using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieBookings")]
public class AppCaddieBooking : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string BookingCode { get; set; } = null!;

    public Guid CustomerId { get; set; }

    [Required]
    [StringLength(255)]
    public string CustomerName { get; set; } = null!;

    [Required]
    [StringLength(20)]
    public string Phone { get; set; } = null!;

    public Guid GolfCourseId { get; set; }

    public Guid CaddieId { get; set; }

    public Guid ScheduleId { get; set; }

    public DateTime BookingDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public int? NumberOfHoles { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public byte Status { get; set; } = 1;

    public byte PaymentStatus { get; set; } = 1;

    public byte CheckinStatus { get; set; } = 1;

    public DateTime? CheckinTime { get; set; }

    [StringLength(1000)]
    public string? CancelReason { get; set; }

    public virtual AppCaddie? Caddie { get; set; }
    public virtual AppCaddieSchedule? Schedule { get; set; }
}
