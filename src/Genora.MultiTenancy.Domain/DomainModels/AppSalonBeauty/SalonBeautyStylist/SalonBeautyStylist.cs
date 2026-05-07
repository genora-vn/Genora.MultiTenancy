using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSalonBeauty;

[Table("AppSalonBeautyStylists")]
public class SalonBeautyStylist : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(255)]
    public string DisplayName { get; set; } = null!;

    [StringLength(500)]
    public string? Avatar { get; set; }

    [StringLength(15)]
    public string? Phone { get; set; }

    public byte? Gender { get; set; }

    public byte? Role { get; set; }

    public byte? Level { get; set; }

    public int ExperienceYear { get; set; }

    public decimal RatingAvg { get; set; }

    public int TotalBooking { get; set; }

    public byte Status { get; set; } = 1;

    public bool IsShowOnApp { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    public int SortOrder { get; set; }

    public virtual ICollection<SalonBeautyBooking> Bookings { get; set; } = new List<SalonBeautyBooking>();
}
