using System.ComponentModel.DataAnnotations;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class CreateHomePageWidgetDto
{
    [Required]
    public string WidgetKey { get; set; } = default!; // ví dụ: Banner, Weather3Days,...

    public string ModuleKey { get; set; } = "Free";
    public bool IsEnabled { get; set; } = true;

    public string? Title { get; set; }
    public int? Limit { get; set; }
    public string? ConfigJson { get; set; }

    // Optional: Có thể set order thủ công; nếu null -> auto
    public int? DisplayOrder { get; set; }
}