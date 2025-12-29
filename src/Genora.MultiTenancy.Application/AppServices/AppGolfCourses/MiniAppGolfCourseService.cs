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

        public async Task<MiniAppGolfCourseDetailDto> GetAsync(Guid id)
        {
            var golfCourse = await  _golfCourseRepository.GetAsync(id);
            return new MiniAppGolfCourseDetailDto { Data = ObjectMapper.Map<GolfCourse, AppGolfCourseDto>(golfCourse), Error = 0, Message = "Success" };
        }

        public async Task<MiniAppGolfCourseListDto> GetListAsync(GetMiniAppGolfCourseListInput input)
        {
            var query = await _golfCourseRepository.GetQueryableAsync();
            if (!string.IsNullOrWhiteSpace(input.GolfCourseSearch))
            {
                query = query.Where(gc => gc.Name.Contains(input.GolfCourseSearch) ||
                                          gc.Address.Contains(input.GolfCourseSearch) ||
                                          gc.Province.Contains(input.GolfCourseSearch) ||
                                          gc.Code.Contains(input.GolfCourseSearch) ||
                                          gc.Phone.Contains(input.GolfCourseSearch));
            }
            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var itemDtos = ObjectMapper.Map<List<GolfCourse>, List<AppGolfCourseDto>>(items);
            var dto = new PagedResultDto<AppGolfCourseDto>(total, itemDtos);
            return new MiniAppGolfCourseListDto { Data = dto , Error = 0, Message = "Success"};
        }
    }
}
