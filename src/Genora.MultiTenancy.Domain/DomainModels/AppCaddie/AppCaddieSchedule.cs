using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieSchedules")]
public class AppCaddieSchedule : FullAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CaddieId { get; set; }

    public DateTime WorkDate { get; set; }

    public byte ShiftCode { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public byte SlotStatus { get; set; } = 1;

    public Guid? BookingId { get; set; }

    public bool IsNightShift { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public virtual AppCaddie? Caddie { get; set; }
    public virtual AppCaddieBooking? Booking { get; set; }
}
