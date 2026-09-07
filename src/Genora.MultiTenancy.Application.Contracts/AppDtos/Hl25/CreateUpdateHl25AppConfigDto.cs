using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// DTO ghi cấu hình chung Mini App "Dược Phẩm Hoa Linh 25 Năm".
/// Delta 2026-09 (tinh giản mạnh): chỉ còn Thể lệ (RulesHtml) + thời gian + phạm vi/ĐVTC + bật/tắt.
/// </summary>
public class CreateUpdateHl25AppConfigDto
{
    [StringLength(256)]
    public string? ProgramName { get; set; }

    public string? RulesHtml { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(256)]
    public string? Scope { get; set; }

    [StringLength(256)]
    public string? OrganizerName { get; set; }

    public bool IsActive { get; set; } = true;
}
