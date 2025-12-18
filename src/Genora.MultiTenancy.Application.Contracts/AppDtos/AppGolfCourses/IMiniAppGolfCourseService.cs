using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses
{
    public interface IMiniAppGolfCourseService : IApplicationService
    {
        Task<PagedResultDto<AppGolfCourseDto>> GetListAsync(GetMiniAppGolfCourseListInput input);
        Task<AppGolfCourseDto> GetAsync(Guid id);
    }
}
