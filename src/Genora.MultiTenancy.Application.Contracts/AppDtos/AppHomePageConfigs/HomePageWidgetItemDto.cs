using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class HomePageWidgetItemDto : EntityDto<Guid>
{
    public int DisplayOrder { get; set; }
    public string Text { get; set; } = default!;
    public string? Icon { get; set; }
    public string? Action { get; set; }
    public bool IsEnabled { get; set; }
}