using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbItems;
public class GetFnbItemListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsAvailable { get; set; }
}