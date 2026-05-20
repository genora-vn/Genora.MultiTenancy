using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Genora.MultiTenancy.Enums;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

/// <summary>
/// Lịch làm việc của 1 stylist tại 1 cơ sở trong 1 ngày, theo 1 khung giờ cụ thể.
/// </summary>
[Table("AppSalonBeautyTimeSlots")]
public class SalonBeautyTimeSlot : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid LocationId { get; set; }

    public Guid StylistId { get; set; }

    public DateTime WorkDate { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public SalonBeautyTimeSlotStatus Status { get; set; } = SalonBeautyTimeSlotStatus.On;

    public bool IsShowOnApp { get; set; } = true;

    [StringLength(500)]
    public string? Note { get; set; }

    public virtual SalonBeautyLocation? Location { get; set; }
    public virtual SalonBeautyStylist? Stylist { get; set; }
}
