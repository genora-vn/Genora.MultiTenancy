using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class HomePageWidgetListItemDto : EntityDto<Guid>
{
    public string WidgetKey { get; set; } = default!;
    public string ModuleKey { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public int DisplayOrder { get; set; }

    public string? Title { get; set; }
    public int? Limit { get; set; }
}