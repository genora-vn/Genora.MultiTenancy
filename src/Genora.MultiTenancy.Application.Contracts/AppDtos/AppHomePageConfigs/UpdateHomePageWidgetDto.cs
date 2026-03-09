namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class UpdateHomePageWidgetDto
{
    public string WidgetKey { get; set; } = default!;
    public string ModuleKey { get; set; } = default!;
    public bool IsEnabled { get; set; }

    public string? Title { get; set; }
    public int? Limit { get; set; }
    public string? ConfigJson { get; set; }
}