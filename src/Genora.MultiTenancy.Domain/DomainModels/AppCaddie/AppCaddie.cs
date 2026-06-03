using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

[Table("AppCaddies")]
public class AppCaddie : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    [Required]
    [StringLength(50)]
    public string CaddieCode { get; set; } = null!;

    [Required]
    [StringLength(255)]
    public string CaddieName { get; set; } = null!;

    [StringLength(500)]
    public string? Avatar { get; set; }

    public byte? Gender { get; set; }

    [StringLength(20)]
    public string? Phone { get; set; }

    public Guid? GolfCourseId { get; set; }

    public DateTime? JoinDate { get; set; }

    public int? HeightCm { get; set; }

    [Column(TypeName = "decimal(3,1)")]
    public decimal RatingAvg { get; set; }

    public int TotalBooking { get; set; }

    public byte Status { get; set; } = 1;

    public bool IsShowOnApp { get; set; }

    [StringLength(1000)]
    public string? Note { get; set; }

    public virtual ICollection<AppCaddieLanguage> Languages { get; set; } = new List<AppCaddieLanguage>();
    public virtual ICollection<AppCaddieVoiceRegion> VoiceRegions { get; set; } = new List<AppCaddieVoiceRegion>();
    public virtual ICollection<AppCaddieSchedule> Schedules { get; set; } = new List<AppCaddieSchedule>();

    // ABP requires constructor with Id for Entity<Guid>
    public AppCaddie()
    {
    }

    public AppCaddie(Guid id) : base(id)
    {
    }
}
