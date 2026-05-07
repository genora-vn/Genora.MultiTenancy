using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.SalonBeautyDtos;

public class GetSalonBeautyListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
}
