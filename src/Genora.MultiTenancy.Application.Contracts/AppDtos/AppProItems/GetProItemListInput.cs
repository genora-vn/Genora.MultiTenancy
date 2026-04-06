using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppProItems;

public class GetProItemListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public Guid? CategoryId { get; set; }
    public bool? IsActive { get; set; }
    public bool? IsAvailable { get; set; }
}
