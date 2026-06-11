using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddieBookingDetails")]
public class AppCaddieBookingDetail : CreationAuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CaddieBookingId { get; set; }

    public Guid CaddieId { get; set; }

    public Guid ScheduleId { get; set; }

    /// <summary>
    /// Per-caddy status within the booking (1=Active, 2=Cancelled)
    /// </summary>
    public byte Status { get; set; } = 1;

    [StringLength(500)]
    public string? Note { get; set; }

    public virtual AppCaddieBooking? CaddieBooking { get; set; }
    public virtual AppCaddie? Caddie { get; set; }
    public virtual AppCaddieSchedule? Schedule { get; set; }

    protected AppCaddieBookingDetail() { }

    public AppCaddieBookingDetail(Guid id, Guid caddieBookingId, Guid caddieId, Guid scheduleId)
        : base(id)
    {
        CaddieBookingId = caddieBookingId;
        CaddieId = caddieId;
        ScheduleId = scheduleId;
    }
}
