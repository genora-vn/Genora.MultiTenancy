using Genora.MultiTenancy.Enums;
using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppCalendarSlots;

public class AppCalendarSlotDto : FullAuditedEntityDto<Guid>
{
    public Guid? TenantId { get; set; }

    public Guid GolfCourseId { get; set; }

    public string GolfCourseName { get; set; }

    public DateTime ApplyDate { get; set; }

    public TimeSpan TimeFrom { get; set; }

    public TimeSpan TimeTo { get; set; }

    public PromotionType PromotionType { get; set; }

    public int MaxSlots { get; set; }

    public string InternalNote { get; set; }

    public bool IsActive { get; set; }

    public List<AppCalendarSlotPriceDto> Prices { get; set; } = new();
}