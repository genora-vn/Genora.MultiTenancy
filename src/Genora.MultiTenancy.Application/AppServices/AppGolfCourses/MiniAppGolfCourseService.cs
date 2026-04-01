using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppOptionExtend;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Enums;
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
        private readonly IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> _promotionTypeRepository;

        public MiniAppGolfCourseService(
            IRepository<GolfCourse, Guid> golfCourseRepository,
            IRepository<DomainModels.AppPromotionTypes.PromotionType, Guid> promotionTypeRepository)
        {
            _golfCourseRepository = golfCourseRepository;
            _promotionTypeRepository = promotionTypeRepository;
        }

        public async Task<MiniAppGolfCourseDetailDto> GetAsync(Guid id)
        {
            var golfCourse = await _golfCourseRepository.GetAsync(id);
            var dto = ObjectMapper.Map<GolfCourse, GolfCourseListData>(golfCourse);

            var promotionMap = await BuildPromotionTypeMapAsync(new[] { golfCourse.PromotionTypeIds });
            dto.PromotionTypes = dto.PromotionTypeIdList
                .Where(x => promotionMap.ContainsKey(x))
                .Select(x => promotionMap[x])
                .ToList();

            return new MiniAppGolfCourseDetailDto
            {
                Data = dto,
                Error = 0,
                Message = "Success"
            };
        }

        public async Task<MiniAppGolfCourseListDto> GetListAsync(GetMiniAppGolfCourseListInput input)
        {
            var query = await _golfCourseRepository.GetQueryableAsync();

            if (!string.IsNullOrWhiteSpace(input.GolfCourseSearch))
            {
                var keyword = input.GolfCourseSearch.Trim();
                query = query.Where(gc =>
                    gc.Name.Contains(keyword) ||
                    (gc.Address != null && gc.Address.Contains(keyword)) ||
                    (gc.Province != null && gc.Province.Contains(keyword)) ||
                    gc.Code.Contains(keyword) ||
                    (gc.Phone != null && gc.Phone.Contains(keyword)));
            }

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            var itemDtos = ObjectMapper.Map<List<GolfCourse>, List<GolfCourseListData>>(items);

            var promotionMap = await BuildPromotionTypeMapAsync(items.Select(x => x.PromotionTypeIds));

            foreach (var dto in itemDtos)
            {
                dto.PromotionTypes = dto.PromotionTypeIdList
                    .Where(x => promotionMap.ContainsKey(x))
                    .Select(x => promotionMap[x])
                    .ToList();
            }

            var dtoResult = new PagedResultDto<GolfCourseListData>(total, itemDtos);
            return new MiniAppGolfCourseListDto
            {
                Data = dtoResult,
                Error = 0,
                Message = "Success"
            };
        }

        public async Task<List<UlitityDto>> GetListUlitities()
        {
            var ulitities = UlititiesEnum.List()
                .Select(x => new UlitityDto { Id = x.Value, Name = x.Name })
                .ToList();

            return ulitities;
        }

        private async Task<Dictionary<Guid, GolfCoursePromotionTypeMiniDto>> BuildPromotionTypeMapAsync(IEnumerable<string?> csvValues)
        {
            var ids = csvValues
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .SelectMany(x => x!.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
                .Where(x => x != Guid.Empty)
                .Distinct()
                .ToList();

            if (ids.Count == 0)
            {
                return new Dictionary<Guid, GolfCoursePromotionTypeMiniDto>();
            }

            var query = await _promotionTypeRepository.GetQueryableAsync();
            var promotions = await AsyncExecuter.ToListAsync(
                query.Where(x => ids.Contains(x.Id) && x.Status)
            );

            return promotions.ToDictionary(
                x => x.Id,
                x => new GolfCoursePromotionTypeMiniDto
                {
                    Id = x.Id,
                    Code = x.Code,
                    Name = x.Name,
                    Description = x.Description,
                    IconUrl = x.IconUrl,
                    ColorCode = x.ColorCode
                });
        }
    }
}