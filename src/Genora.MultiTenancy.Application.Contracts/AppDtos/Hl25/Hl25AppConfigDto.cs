using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// DTO đọc cấu hình chung Mini App "Dược Phẩm Hoa Linh 25 Năm".
/// </summary>
public class Hl25AppConfigDto : AuditedEntityDto<Guid>
{
    public string? ProgramName { get; set; }
    public string? IntroductionHtml { get; set; }
    public string? Format { get; set; }
    public string? GiftDeliveryTime { get; set; }
    public string? RulesHtml { get; set; }
    public DateTime? StartTime { get; set; }
    public DateTime? EndTime { get; set; }
    public string? Scope { get; set; }
    public string? OrganizerName { get; set; }
    public bool IsActive { get; set; }
}
