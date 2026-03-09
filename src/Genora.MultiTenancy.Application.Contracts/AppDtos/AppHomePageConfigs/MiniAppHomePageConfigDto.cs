using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class MiniAppHomePageConfigDto
{
    public string ThemeKey { get; set; } = "default";
    public List<MiniAppHomePageWidgetDto> Widgets { get; set; } = new();
}

public class MiniAppHomePageWidgetDto
{
    public Guid Id { get; set; }
    public string WidgetKey { get; set; } = default!;
    public string ModuleKey { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public int DisplayOrder { get; set; }
    public string? Title { get; set; }
    public int? Limit { get; set; }
    public string? ConfigJson { get; set; }
}