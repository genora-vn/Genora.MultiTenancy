using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.DomainModels.AppNews;
using System;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using System.Linq;
using System.Collections.Generic;

namespace Genora.MultiTenancy.AppServices.AppNewsServices
{
    public class MiniAppNewsService : ApplicationService, IMiniAppNewsService
    {
        private readonly IRepository<News, Guid> _newsRepository;

        public MiniAppNewsService(IRepository<News, Guid> newsRepository)
        {
            _newsRepository = newsRepository;
        }

        public async Task<MiniAppNewsDetailDto> GetAsync(Guid id)
        {
            var news = await _newsRepository.GetAsync(id);
            var result = ObjectMapper.Map<News, MiniAppNewsData>(news);
            return new MiniAppNewsDetailDto { Data = result , Error= 0, Message = "Success"};
        }

        public async Task<MiniAppNewsListDto> GetListAsync(GetMiniAppNewsDto input)
        {
            var queries = await _newsRepository.GetQueryableAsync();
            var query = queries;
            if (!input.FilterText.IsNullOrWhiteSpace())
            {
                var filter = input.FilterText.Trim();
                query = query.Where(x => x.Title.Contains(filter) || x.ShortDescription.Contains(filter));
            }

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(News.DisplayOrder) + " asc, " + nameof(News.PublishedAt) + " desc"
                : input.Sorting;

            query = query.OrderBy(sorting);

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter
            .ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var dtoList = ObjectMapper.Map<List<News>, List<MiniAppNewsData>>(items);
            var result = new PagedResultDto<MiniAppNewsData>(total, dtoList);
            return new MiniAppNewsListDto { Data = result , Error = 0, Message = "Success" };

        }
    }
}
