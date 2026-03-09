using System;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class UpdateFeatureGridItemDto
{
    public Guid Id { get; set; } // Guid.Empty => create new
    public int DisplayOrder { get; set; }
    public string Text { get; set; } = default!;
    public string? Icon { get; set; }
    public string? Action { get; set; }
    public bool IsEnabled { get; set; }
}