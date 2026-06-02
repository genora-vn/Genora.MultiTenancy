using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.Caddies;

public class GetCaddieListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
    public Guid? GolfCourseId { get; set; }
    public byte? Status { get; set; }
    public bool? IsShowOnApp { get; set; }
}
