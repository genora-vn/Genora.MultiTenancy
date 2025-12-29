using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppNews
{
    public class GetMiniAppNewsDto : PagedAndSortedResultRequestDto
    {
        public string? FilterText { get; set; }
    }
}
