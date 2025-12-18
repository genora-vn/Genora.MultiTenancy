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

        public async Task<AppNewsDto> GetAsync(Guid id)
        {
            var news = await _newsRepository.GetAsync(id);
            return ObjectMapper.Map<News, AppNewsDto>(news);
        }

        public async Task<PagedResultDto<AppNewsDto>> GetListAsync(GetNewsListInput input)
        {
            var queries = await _newsRepository.GetQueryableAsync();
            var query = queries;
            if (!input.FilterText.IsNullOrWhiteSpace())
            {
                var filter = input.FilterText.Trim();
                query = query.Where(x => x.Title.Contains(filter));
            }

            if (input.Status.HasValue)
            {
                query = query.Where(x => x.Status == (byte)input.Status.Value);
            }

            if (input.PublishedAtFrom.HasValue)
            {
                query = query.Where(x => x.PublishedAt >= input.PublishedAtFrom.Value);
            }

            if (input.PublishedAtTo.HasValue)
            {
                query = query.Where(x => x.PublishedAt <= input.PublishedAtTo.Value);
            }

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(News.DisplayOrder) + " asc, " + nameof(News.PublishedAt) + " desc"
                : input.Sorting;

            query = query.OrderBy(sorting);

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter
            .ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
            var dtoList = ObjectMapper.Map<List<News>, List<AppNewsDto>>(items);
            return new PagedResultDto<AppNewsDto>(total, dtoList);

        }
    }
}
