using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.ZaloAuths;

public class GetAppZaloAuthListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public bool? IsActive { get; set; }
}