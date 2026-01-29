using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.Enums;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.AppServices.AppNewsServices
{
    public class MiniAppNewsService : ApplicationService, IMiniAppNewsService
    {
        private readonly IRepository<News, Guid> _newsRepository;
        private readonly IRepository<NewsRelated, Guid> _newsRelatedRepository;
        private readonly IConfiguration _configuration;

        public MiniAppNewsService(IRepository<News, Guid> newsRepository, IConfiguration configuration, IRepository<NewsRelated, Guid> newsRelatedRepository)
        {
            _newsRepository = newsRepository;
            _configuration = configuration;
            _newsRelatedRepository = newsRelatedRepository;
        }

        public async Task<MiniAppNewsDetailDto> GetAsync(Guid id)
        {
            // 1) bài chính
            var news = await _newsRepository.GetAsync(id);
            var result = ObjectMapper.Map<News, MiniAppNewsData>(news);
            result.ThumbnailUrl = NormalizeThumb(result.ThumbnailUrl);

            var relRows = await _newsRelatedRepository.GetListAsync(x => x.NewsId == id);
            var relIds = relRows
                .Select(x => x.RelatedNewsId)
                .Where(x => x != Guid.Empty && x != id)
                .Distinct()
                .ToList();

            if (relIds.Count > 0)
            {
                var q = await _newsRepository.GetQueryableAsync();

                var relatedEntities = await AsyncExecuter.ToListAsync(
                    q.Where(x => relIds.Contains(x.Id))
                     .Where(x => x.Status == (byte)NewsStatus.Published)
                     .OrderBy(nameof(News.DisplayOrder) + " asc, " + nameof(News.PublishedAt) + " desc")
                );

                var relatedDtos = ObjectMapper.Map<List<News>, List<MiniAppRelatedNewsData>>(relatedEntities);
                foreach (var r in relatedDtos)
                {
                    r.ThumbnailUrl = NormalizeThumb(r.ThumbnailUrl);
                }

                result.RelatedNews = relatedDtos;
            }
            else
            {
                result.RelatedNews = new List<MiniAppRelatedNewsData>();
            }

            return new MiniAppNewsDetailDto { Data = result, Error = 0, Message = "Success" };
        }

        public async Task<MiniAppNewsListDto> GetListAsync(GetMiniAppNewsDto input)
        {
            var queries = await _newsRepository.GetQueryableAsync();

            var query = queries.Where(x => x.Status == (byte)NewsStatus.Published);

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

            var pageEntities = await AsyncExecuter
                .ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

            var dtoList = ObjectMapper.Map<List<News>, List<MiniAppNewsData>>(pageEntities);

            foreach (var item in dtoList)
            {
                item.ThumbnailUrl = NormalizeThumb(item.ThumbnailUrl);
            }

            var newsIds = dtoList.Select(x => x.Id).Distinct().ToList();
            if (newsIds.Count > 0)
            {
                var relQ = await _newsRelatedRepository.GetQueryableAsync();
                var relRows = await AsyncExecuter.ToListAsync(relQ.Where(r => newsIds.Contains(r.NewsId)));

                if (relRows.Count > 0)
                {
                    var relatedIds = relRows.Select(r => r.RelatedNewsId).Distinct().ToList();

                    var relatedNewsQ = await _newsRepository.GetQueryableAsync();
                    var relatedEntities = await AsyncExecuter.ToListAsync(
                        relatedNewsQ.Where(n => relatedIds.Contains(n.Id) && n.Status == (byte)NewsStatus.Published)
                    );

                    var relatedDtos = ObjectMapper.Map<List<News>, List<MiniAppRelatedNewsData>>(relatedEntities);

                    foreach (var r in relatedDtos)
                    {
                        r.ThumbnailUrl = NormalizeThumb(r.ThumbnailUrl);
                    }

                    var relatedDict = relatedDtos.ToDictionary(x => x.Id, x => x);

                    var relByNews = relRows
                        .GroupBy(r => r.NewsId)
                        .ToDictionary(g => g.Key, g => g.Select(x => x.RelatedNewsId).Distinct().ToList());

                    foreach (var item in dtoList)
                    {
                        if (relByNews.TryGetValue(item.Id, out var rids))
                        {
                            item.RelatedNews = rids
                                .Where(id => relatedDict.ContainsKey(id))
                                .Select(id => relatedDict[id])
                                .OrderBy(x => x.DisplayOrder)
                                .ThenByDescending(x => x.PublishedAt)
                                .ToList();
                        }
                    }
                }
            }

            var result = new PagedResultDto<MiniAppNewsData>(total, dtoList);
            return new MiniAppNewsListDto { Data = result, Error = 0, Message = "Success" };
        }

        private string? NormalizeThumb(string? url)
        {
            if (!string.IsNullOrEmpty(url) && url.StartsWith("/uploads"))
            {
                return _configuration["App:AppUrl"] + url;
            }
            return url;
        }
    }
}
