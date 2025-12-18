using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppGolfCourses
{
    public class MiniAppGolfCourseService : ApplicationService, IMiniAppGolfCourseService
    {
        private readonly IRepository<GolfCourse, Guid> _golfCourseRepository;

        public MiniAppGolfCourseService(IRepository<GolfCourse, Guid> golfCourseRepository)
        {
            _golfCourseRepository = golfCourseRepository;
        }

        public async Task<AppGolfCourseDto> GetAsync(Guid id)
        {
            var golfCourse = await  _golfCourseRepository.GetAsync(id);
            return ObjectMapper.Map<GolfCourse, AppGolfCourseDto>(golfCourse);
        }

        public async Task<PagedResultDto<AppGolfCourseDto>> GetListAsync(GetMiniAppGolfCourseListInput input)
        {
            var query = await _golfCourseRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(input.GolfCourseSearch))
            {
                query = query.Where(gc => gc.Name.Contains(input.GolfCourseSearch) ||
                                          gc.Address.Contains(input.GolfCourseSearch) ||
                                          gc.Province.Contains(input.GolfCourseSearch) ||
                                          gc.Phone.Contains(input.GolfCourseSearch));
            }
            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var itemDtos = ObjectMapper.Map<List<GolfCourse>, List<AppGolfCourseDto>>(items);
            return new PagedResultDto<AppGolfCourseDto>(total, itemDtos);
        }
    }
}
