using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public class MiniAppFnbCategoryListDto : ZaloBaseResponse
{
    public List<MiniAppFnbCategoryData> Data { get; set; } = new();
}

public class MiniAppFnbCategoryData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public int SortOrder { get; set; }
}