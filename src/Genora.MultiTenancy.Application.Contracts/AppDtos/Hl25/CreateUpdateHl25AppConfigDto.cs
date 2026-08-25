using System;
using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.Hl25;

/// <summary>
/// DTO ghi cấu hình chung Mini App "Dược Phẩm Hoa Linh 25 Năm".
/// Các trường HTML (RulesHtml/GamePlayHtml/TvcHtml) nhận nội dung từ Summernote.
/// </summary>
public class CreateUpdateHl25AppConfigDto
{
    [StringLength(256)]
    public string? ProgramName { get; set; }

    [StringLength(1024)]
    public string? LogoUrl { get; set; }

    [StringLength(1024)]
    public string? BannerUrl { get; set; }

    [StringLength(1024)]
    public string? TvcUrl { get; set; }

    public string? TvcHtml { get; set; }

    public string? RulesHtml { get; set; }

    public string? GamePlayHtml { get; set; }

    public DateTime? StartTime { get; set; }

    public DateTime? EndTime { get; set; }

    [StringLength(256)]
    public string? Scope { get; set; }

    [StringLength(256)]
    public string? OrganizerName { get; set; }

    public bool IsActive { get; set; } = true;
}
