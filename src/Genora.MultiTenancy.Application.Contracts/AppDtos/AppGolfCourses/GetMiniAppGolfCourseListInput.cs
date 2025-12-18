using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses
{
    public class GetMiniAppGolfCourseListInput : PagedAndSortedResultRequestDto
    {
        public string? GolfCourseSearch { get; set; }
    }
}
