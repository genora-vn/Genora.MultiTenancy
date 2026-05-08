using System;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeautyDtos;

public class GetSalonBeautyListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? CustomerGroup { get; set; }
    public byte? Source { get; set; }
    public byte? Status { get; set; }
}
