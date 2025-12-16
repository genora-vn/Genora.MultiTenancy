using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppNews;
public class GetNewsListInput : PagedAndSortedResultRequestDto
{
    public string FilterText { get; set; }   // search theo title

    public NewsStatus? Status { get; set; }

    public DateTime? PublishedAtFrom { get; set; }
    public DateTime? PublishedAtTo { get; set; }

    public bool? IsActive { get; set; }
}
