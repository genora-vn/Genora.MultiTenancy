using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Volo.Abp.Domain.Entities.Auditing;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.DomainModels.AppCaddie;

/// <summary>
/// Lưu template lịch làm việc theo pattern tuần (DayOfWeek 0=CN, 1=T2,...6=T7).
/// Admin save 1 lần, apply cho nhiều tuần tiếp theo.
/// </summary>
[Table("AppCaddieScheduleTemplates")]
public class AppCaddieScheduleTemplate : AuditedEntity<Guid>, IMultiTenant
{
    public Guid? TenantId { get; set; }

    public Guid CaddieId { get; set; }

    /// <summary>Ngày trong tuần: 0=Chủ nhật, 1=Thứ 2,...6=Thứ 7</summary>
    public byte DayOfWeek { get; set; }

    public byte ShiftCode { get; set; }

    public TimeSpan StartTime { get; set; }

    public TimeSpan EndTime { get; set; }

    public byte SlotStatus { get; set; } = 1;

    public bool IsNightShift { get; set; }

    [StringLength(500)]
    public string? Note { get; set; }

    [StringLength(100)]
    public string? TemplateName { get; set; }
}
