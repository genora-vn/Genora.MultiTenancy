
using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Volo.Abp.Application.Dtos;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses
{
    public class MiniAppGolfCourseListDto : ZaloBaseResponse
    {
        public PagedResultDto<AppGolfCourseDto> Data { get; set; }
    }
}
