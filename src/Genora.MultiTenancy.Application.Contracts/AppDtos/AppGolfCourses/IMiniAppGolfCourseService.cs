using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses
{
    public interface IMiniAppGolfCourseService : IApplicationService
    {
        Task<MiniAppGolfCourseListDto> GetListAsync(GetMiniAppGolfCourseListInput input);
        Task<MiniAppGolfCourseDetailDto> GetAsync(Guid id);
    }
}
