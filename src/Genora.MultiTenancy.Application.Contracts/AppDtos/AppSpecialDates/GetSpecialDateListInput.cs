using Genora.MultiTenancy.Enums;
using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppSpecialDates;
public class GetSpecialDateListInput : PagedAndSortedResultRequestDto
{
    public string? Filter { get; set; }
}