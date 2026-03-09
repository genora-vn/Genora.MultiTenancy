using System;
using System.Collections.Generic;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class HomePageWidgetDto : EntityDto<Guid>
{
    public Guid AppHomePageConfigId { get; set; }
    public string WidgetKey { get; set; } = default!;
    public string ModuleKey { get; set; } = default!;
    public bool IsEnabled { get; set; }
    public int DisplayOrder { get; set; }

    public string? Title { get; set; }
    public int? Limit { get; set; }
    public string? ConfigJson { get; set; }

    public List<HomePageWidgetItemDto> Items { get; set; } = new();
}