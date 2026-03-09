using System;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class UpdateWidgetRequestDto
{
    public Guid Id { get; set; }

    public bool? IsEnabled { get; set; }
    public int? DisplayOrder { get; set; }

    public string? ModuleKey { get; set; }
    public string? Title { get; set; }
    public int? Limit { get; set; }
    public string? ConfigJson { get; set; }
}