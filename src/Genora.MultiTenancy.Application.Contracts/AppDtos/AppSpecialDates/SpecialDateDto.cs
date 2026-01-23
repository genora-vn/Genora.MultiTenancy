using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppSpecialDates;

public class SpecialDateDto : FullAuditedEntityDto<Guid>
{
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public List<DateTime>? Dates { get; set; }
    public Guid? GolfCourseId { get; set; }
    public bool IsActive { get; set; }
}