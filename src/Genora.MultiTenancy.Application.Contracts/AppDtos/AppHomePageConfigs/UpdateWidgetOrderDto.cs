using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppHomePageConfigs;
public class UpdateWidgetOrderDto
{
    public List<Guid> OrderedIds { get; set; } = new();
}