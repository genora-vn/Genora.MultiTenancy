using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class UpdateFeatureGridDto
{
    public string? Title { get; set; }
    public int? Limit { get; set; }
    public List<UpdateFeatureGridItemDto> Items { get; set; } = new();
}