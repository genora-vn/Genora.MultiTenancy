using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>DTO đọc một ô trên vòng quay.</summary>
public class Hl25WheelSlotDto : EntityDto<Guid>
{
    public Guid WheelConfigId { get; set; }
    public Guid? GiftId { get; set; }
    public string? Label { get; set; }
    public string? SlotImageUrl { get; set; }
    public decimal WinRate { get; set; }
    public int DisplayOrder { get; set; }
    public string? ColorHex { get; set; }
}

/// <summary>DTO đọc cấu hình vòng quay (kèm danh sách ô).</summary>
public class Hl25WheelConfigDto : AuditedEntityDto<Guid>
{
    public string? Title { get; set; }
    public string? SubTitle { get; set; }
    public string? PrimaryColor { get; set; }
    public string? SecondaryColor { get; set; }
    public string? BackgroundImageUrl { get; set; }
    public string? PointerImageUrl { get; set; }
    public int SlotCount { get; set; }
    public bool IsActive { get; set; }
    public List<Hl25WheelSlotDto> Slots { get; set; } = new();
}
