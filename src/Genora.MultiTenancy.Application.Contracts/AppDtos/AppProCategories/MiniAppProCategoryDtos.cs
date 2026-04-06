using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppDtos.AppProCategories;

public class MiniAppProCategoryListDto : ZaloBaseResponse
{
    public List<MiniAppProCategoryData> Data { get; set; } = new();
}

public class MiniAppProCategoryData
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string? Code { get; set; }
    public int SortOrder { get; set; }
}
