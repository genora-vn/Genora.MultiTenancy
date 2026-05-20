using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
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
            var news = await _newsRepository.FirstOrDefaultAsync(x =>
                x.Id == id &&
                x.Status == (byte)NewsStatus.Published
            );

            if (news == null)
            {
                throw new Exception($"News with id {id} not found");
            }

            var result = ObjectMapper.Map<News, MiniAppNewsData>(news);
            result.ThumbnailUrl = ImageHelper.NormalizeThumb(_configuration, result.ThumbnailUrl);

            var relRows = await _newsRelatedRepository.GetListAsync(x => x.NewsId == id);

            var relIds = relRows
                .Select(x => x.RelatedNewsId)
                .Where(x => x != Guid.Empty && x != id)
                .Distinct()
                .ToList();

            if (relIds.Count > 0)
            {
                var relatedQuery = await _newsRepository.GetQueryableAsync();

                var relatedDtos = await AsyncExecuter.ToListAsync(
                    relatedQuery
                        .Where(x => relIds.Contains(x.Id) && x.Status == (byte)NewsStatus.Published)
                        .OrderBy(nameof(News.DisplayOrder) + " asc, " + nameof(News.PublishedAt) + " desc")
                        .Select(x => new MiniAppRelatedNewsData
                        {
                            Id = x.Id,

                            Title = x.Title,
                            ShortDescription = x.ShortDescription,
                            ThumbnailUrl = x.ThumbnailUrl,

                            PublishedAt = x.PublishedAt,
                            DisplayOrder = x.DisplayOrder
                        })
                );

                foreach (var r in relatedDtos)
                {
                    r.ThumbnailUrl = ImageHelper.NormalizeThumb(_configuration, r.ThumbnailUrl);
                }

                result.RelatedNews = relatedDtos;
            }
            else
            {
                result.RelatedNews = new List<MiniAppRelatedNewsData>();
            }

            return new MiniAppNewsDetailDto
            {
                Data = result,
                Error = 0,
                Message = "Success"
            };
        }

        public async Task<MiniAppNewsListDto> GetListAsync(GetMiniAppNewsDto input)
        {
            var queryable = await _newsRepository.GetQueryableAsync();

            var query = queryable.Where(x => x.Status == (byte)NewsStatus.Published);

            if (!input.FilterText.IsNullOrWhiteSpace())
            {
                var filter = input.FilterText.Trim();
                query = query.Where(x =>
                    x.Title.Contains(filter) ||
                    x.ShortDescription.Contains(filter)
                );
            }

            var total = await AsyncExecuter.CountAsync(query);

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? nameof(News.DisplayOrder) + " asc, " + nameof(News.PublishedAt) + " desc, " + nameof(News.CreationTime) + " desc"
                : input.Sorting;

            var dtoList = await AsyncExecuter.ToListAsync(
                query
                    .OrderBy(sorting)
                    .Skip(input.SkipCount)
                    .Take(input.MaxResultCount)
                    .Select(x => new MiniAppNewsData
                    {
                        Id = x.Id,

                        Title = x.Title,
                        ShortDescription = x.ShortDescription,
                        ThumbnailUrl = x.ThumbnailUrl,

                        // Quan trọng: list không trả HTML nặng
                        ContentHtml = string.Empty,

                        PublishedAt = x.PublishedAt,
                        Status = x.Status,
                        DisplayOrder = x.DisplayOrder,

                        RelatedNews = new List<MiniAppRelatedNewsData>()
                    })
            );

            foreach (var item in dtoList)
            {
                item.ThumbnailUrl = ImageHelper.NormalizeThumb(_configuration, item.ThumbnailUrl);
            }

            var result = new PagedResultDto<MiniAppNewsData>(total, dtoList);

            return new MiniAppNewsListDto
            {
                Data = result,
                Error = 0,
                Message = "Success"
            };
        }
    }
}
