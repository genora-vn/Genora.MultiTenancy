using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppGolfCourses;

public interface IAppGolfCourseService :
        ICrudAppService<
            AppGolfCourseDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAppGolfCourseDto>
{
}