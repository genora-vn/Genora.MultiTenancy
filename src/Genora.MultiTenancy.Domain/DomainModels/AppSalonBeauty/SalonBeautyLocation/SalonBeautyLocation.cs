using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyLocations")]
public class SalonBeautyLocation : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(255)]
    public string Name { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Address { get; set; } = null!;

    [StringLength(15)]
    public string? Phone { get; set; }

    public TimeSpan OpenTime { get; set; }

    public TimeSpan CloseTime { get; set; }

    /// <summary>
    /// Thời gian giữa các slot (phút). VD: 60 = mỗi 1 tiếng.
    /// </summary>
    public int SlotDuration { get; set; } = 60;

    /// <summary>
    /// Thời gian nghỉ giữa 2 slot (phút). VD: 10 phút nghỉ. Mặc định 0.
    /// </summary>
    public int BufferTime { get; set; } = 0;

    /// <summary>
    /// Số khách tối đa / 1 slot. Default = 1.
    /// </summary>
    public int MaxCapacityPerSlot { get; set; } = 1;

    [StringLength(500)]
    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsShowOnApp { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<SalonBeautyTimeSlot> TimeSlots { get; set; } = new List<SalonBeautyTimeSlot>();
}
