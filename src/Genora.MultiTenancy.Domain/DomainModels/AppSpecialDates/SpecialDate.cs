using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppSpecialDates;

[Table("AppSpecialDates")]
public class SpecialDate : FullAuditedAggregateRoot<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid? GolfCourseId { get; set; } // nullable: cấu hình chung hoặc theo sân

    [Required]
    [StringLength(50)]
    public string Name { get; set; } = default!;
    // "Ngày trong tuần" | "Ngày cuối tuần" | "Ngày lễ" | "Member day"

    [StringLength(500)]
    public string? Description { get; set; }

    // JSON: ["2026-01-01","2026-02-10",...]
    public string? DatesJson { get; set; }

    // NEW: bitmask Mon..Sun (0..6). All = 127. Null when Name = "Ngày lễ".
    public int? WeekdaysMask { get; set; }

    public bool IsActive { get; set; } = true;

    protected SpecialDate() { }

    public SpecialDate(Guid id, string name, string? description, Guid? golfCourseId, string? datesJson)
        : base(id)
    {
        Name = name;
        Description = description;
        GolfCourseId = golfCourseId;
        DatesJson = datesJson;
    }
}