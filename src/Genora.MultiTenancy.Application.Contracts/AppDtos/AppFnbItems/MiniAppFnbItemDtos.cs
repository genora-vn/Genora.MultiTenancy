using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public class GetMiniAppFnbItemListInput : PagedAndSortedResultRequestDto
{
    public Guid? CategoryId { get; set; }
    public string? FilterText { get; set; }
    public bool? IsAvailable { get; set; }
}

public class MiniAppFnbItemListDto : ZaloBaseResponse
{
    public PagedResultDto<MiniAppFnbItemData> Data { get; set; } = null!;
}

public class MiniAppFnbItemDetailDto : ZaloBaseResponse
{
    public MiniAppFnbItemData Data { get; set; } = null!;
}

public class MiniAppFnbItemData
{
    public Guid Id { get; set; }
    public Guid CategoryId { get; set; }
    public string? CategoryName { get; set; }
    public string Name { get; set; } = null!;
    public decimal Price { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public bool IsAvailable { get; set; }
    public int SortOrder { get; set; }
}