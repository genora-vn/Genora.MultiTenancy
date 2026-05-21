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

    /// <summary>
    /// Số khách tối đa được phép book trong slot (không vượt quá Location.MaxCapacityPerSlot).
    /// </summary>
    public int Capacity { get; set; } = 1;

    /// <summary>
    /// Số khách đã book hiện tại. Khi BookedCount &gt;= Capacity → status sẽ tự chuyển Full.
    /// </summary>
    public int BookedCount { get; set; } = 0;

    /// <summary>
    /// Manual override flag - admin tự can thiệp trạng thái slot, không cho recalculate đè.
    /// </summary>
    public bool IsManualOverride { get; set; } = false;

    public SalonBeautyTimeSlotStatus Status { get; set; } = SalonBeautyTimeSlotStatus.On;

    public bool IsShowOnApp { get; set; } = true;

    [StringLength(500)]
    public string? Note { get; set; }

    public virtual SalonBeautyLocation? Location { get; set; }
    public virtual SalonBeautyStylist? Stylist { get; set; }
}
