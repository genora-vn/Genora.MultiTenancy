using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppFnbCategories;
public class GetFnbCategoryListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public bool? IsActive { get; set; }
}