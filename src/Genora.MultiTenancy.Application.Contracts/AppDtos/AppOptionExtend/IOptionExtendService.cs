using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;

namespace Genora.MultiTenancy.AppDtos.AppOptionExtend
{
    public interface IOptionExtendService : ICrudAppService<AppOptionExtendDto, Guid, GetListOptionExtendInput, CreateUpdateOptionExtendDto>
    {
        Task<List<GolfCourseUtilityDto>> GetUtilitiesAsync();
    }
}
