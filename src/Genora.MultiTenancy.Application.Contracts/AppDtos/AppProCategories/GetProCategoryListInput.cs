using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppProCategories;

public class GetProCategoryListInput : PagedAndSortedResultRequestDto
{
    public string? FilterText { get; set; }
    public bool? IsActive { get; set; }
}
