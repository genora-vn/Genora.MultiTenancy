using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class FeatureGridDto
{
    public string? Title { get; set; }
    public int? Limit { get; set; }
    public List<HomePageWidgetItemDto> Items { get; set; } = new();
}