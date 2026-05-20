using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.DomainModels.AppNews;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Features.AppNewsFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppNewsServices;

[Authorize]
public class AppNewsService :
    FeatureProtectedCrudAppService<News, AppNewsDto, Guid, GetNewsListInput, CreateUpdateAppNewsDto>,
    IAppNewsService
{
    protected override string FeatureName => AppNewsFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppNews.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppNews.Default;

    private readonly IManageImageService _manageImageService;
    private readonly IRepository<NewsRelated, Guid> _newsRelatedRepo;

    private const long MaxImageBytes = 20L * 1024 * 1024; // 20MB
    private const int MaxRelatedNews = 6;

    public AppNewsService(
        IRepository<News, Guid> repository,
        IRepository<NewsRelated, Guid> newsRelatedRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IManageImageService manageImageService)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppNews.Default;
        GetListPolicyName = MultiTenancyPermissions.AppNews.Default;
        CreatePolicyName = MultiTenancyPermissions.AppNews.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppNews.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppNews.Delete;

        _newsRelatedRepo = newsRelatedRepo;
        _manageImageService = manageImageService;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<AppNewsDto>> GetListAsync(GetNewsListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x =>
                x.Title.Contains(filter) ||
                x.ShortDescription.Contains(filter)
            );
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

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(News.DisplayOrder) + " asc, " + nameof(News.PublishedAt) + " desc, " + nameof(News.CreationTime) + " desc"
            : input.Sorting;

        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(sorting)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount)
                .Select(x => new AppNewsDto
                {
                    Id = x.Id,
                    TenantId = x.TenantId,

                    Title = x.Title,
                    ShortDescription = x.ShortDescription,
                    ThumbnailUrl = x.ThumbnailUrl,

                    // Quan trọng: màn danh sách không trả HTML nặng
                    ContentHtml = string.Empty,

                    PublishedAt = x.PublishedAt,
                    Status = x.Status,
                    DisplayOrder = x.DisplayOrder,

                    CreationTime = x.CreationTime,
                    CreatorId = x.CreatorId,
                    LastModificationTime = x.LastModificationTime,
                    LastModifierId = x.LastModifierId
                })
        );

        return new PagedResultDto<AppNewsDto>(totalCount, items);
    }

    public override async Task<AppNewsDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var entity = await Repository.GetAsync(id);
        var dto = ObjectMapper.Map<News, AppNewsDto>(entity);

        var relatedRows = await _newsRelatedRepo.GetListAsync(x => x.NewsId == id);

        dto.RelatedNewsIds = relatedRows
            .Select(x => x.RelatedNewsId)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (dto.RelatedNewsIds.Count > 0)
        {
            var relatedQuery = await Repository.GetQueryableAsync();

            var relatedNews = await AsyncExecuter.ToListAsync(
                relatedQuery
                    .Where(x => dto.RelatedNewsIds.Contains(x.Id))
                    .Select(x => new
                    {
                        x.Id,
                        x.Title
                    })
            );

            dto.RelatedNewsTitles = relatedNews.ToDictionary(x => x.Id, x => x.Title);
        }
        else
        {
            dto.RelatedNewsTitles = new Dictionary<Guid, string>();
        }

        return dto;
    }

    private void ValidateThumbnailAndRelated(CreateUpdateAppNewsDto input, Guid? editingId = null)
    {
        var errors = new List<ValidationResult>();

        if (input.IsUploadImage)
        {
            if (input.Images == null || (input.Images.ContentLength ?? 0) <= 0)
            {
                errors.Add(new ValidationResult(
                    "Vui lòng chọn ảnh để upload trước khi lưu.",
                    new[] { nameof(CreateUpdateAppNewsDto.Images) }
                ));
            }
            else
            {
                var len = input.Images.ContentLength ?? 0;
                if (len > MaxImageBytes)
                {
                    errors.Add(new ValidationResult(
                        "Ảnh vượt quá 20MB. Vui lòng chọn ảnh nhỏ hơn.",
                        new[] { nameof(CreateUpdateAppNewsDto.Images) }
                    ));
                }

                var contentType = input.Images.ContentType ?? "";
                if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    errors.Add(new ValidationResult(
                        "File không phải ảnh hợp lệ.",
                        new[] { nameof(CreateUpdateAppNewsDto.Images) }
                    ));
                }
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(input.ThumbnailUrl))
            {
                errors.Add(new ValidationResult(
                    "Vui lòng nhập URL ảnh đại diện hoặc bật chế độ upload ảnh.",
                    new[] { nameof(CreateUpdateAppNewsDto.ThumbnailUrl) }
                ));
            }
        }

        input.RelatedNewsIds ??= new List<Guid>();

        var distinct = input.RelatedNewsIds
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (distinct.Count > MaxRelatedNews)
        {
            errors.Add(new ValidationResult(
                $"Chỉ được chọn tối đa {MaxRelatedNews} tin liên quan.",
                new[] { nameof(CreateUpdateAppNewsDto.RelatedNewsIds) }
            ));
        }

        if (editingId.HasValue && distinct.Contains(editingId.Value))
        {
            errors.Add(new ValidationResult(
                "Tin liên quan không được trùng với bài viết hiện tại.",
                new[] { nameof(CreateUpdateAppNewsDto.RelatedNewsIds) }
            ));
        }

        input.RelatedNewsIds = distinct;

        if (errors.Count > 0)
        {
            throw new AbpValidationException("Validation failed", errors);
        }
    }

    private List<Guid> NormalizeRelatedIds(Guid newsId, List<Guid>? ids)
    {
        return (ids ?? new List<Guid>())
            .Where(x => x != Guid.Empty && x != newsId)
            .Distinct()
            .Take(MaxRelatedNews)
            .ToList();
    }

    /// <summary>
    /// ✅ HARD DELETE toàn bộ related cũ rồi insert lại danh sách mới.
    /// - Xóa luôn khỏi DB (không soft delete) -> không dính unique index khi chọn lại.
    /// </summary>
    private async Task SaveRelatedAsync(Guid newsId, List<Guid>? relatedIds)
    {
        var ids = NormalizeRelatedIds(newsId, relatedIds);
        var tenantId = CurrentTenant.Id;

        // ✅ Hard delete ALL existing rows (kể cả đã soft delete trước đó)
        // ABP v9 hỗ trợ HardDeleteAsync cho entity có ISoftDelete (FullAuditedEntity có IsDeleted)
        await _newsRelatedRepo.HardDeleteAsync(x => x.NewsId == newsId && x.TenantId == tenantId);

        if (ids.Count == 0) return;

        var rows = ids.Select(rid =>
            new NewsRelated(GuidGenerator.Create(), newsId, rid, tenantId)
        ).ToList();

        await _newsRelatedRepo.InsertManyAsync(rows, autoSave: true);
    }

    public override async Task<AppNewsDto> CreateAsync(CreateUpdateAppNewsDto input)
    {
        await CheckCreatePolicyAsync();

        ValidateThumbnailAndRelated(input);

        var entity = ObjectMapper.Map<CreateUpdateAppNewsDto, News>(input);

        if (!entity.PublishedAt.HasValue && entity.Status == (byte)NewsStatus.Published)
        {
            entity.PublishedAt = Clock.Now;
        }

        if (input.IsUploadImage && input.Images != null && (input.Images.ContentLength ?? 0) > 0)
        {
            var upload = await _manageImageService.UploadImageAsync(input.Images, CurrentTenant.Id.ToString());
            entity.ThumbnailUrl = upload;
        }

        entity = await Repository.InsertAsync(entity, autoSave: true);

        await SaveRelatedAsync(entity.Id, input.RelatedNewsIds);

        var dto = ObjectMapper.Map<News, AppNewsDto>(entity);
        dto.RelatedNewsIds = input.RelatedNewsIds ?? new List<Guid>();
        return dto;
    }

    public override async Task<AppNewsDto> UpdateAsync(Guid id, CreateUpdateAppNewsDto input)
    {
        await CheckUpdatePolicyAsync();

        ValidateThumbnailAndRelated(input, editingId: id);

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);

        if (!entity.PublishedAt.HasValue && entity.Status == (byte)NewsStatus.Published)
        {
            entity.PublishedAt = Clock.Now;
        }

        if (input.IsUploadImage && input.Images != null && (input.Images.ContentLength ?? 0) > 0)
        {
            if (!string.IsNullOrWhiteSpace(entity.ThumbnailUrl) && entity.ThumbnailUrl.StartsWith("/upload"))
            {
                await _manageImageService.DeleteFileAsync(entity.ThumbnailUrl);
            }

            var upload = await _manageImageService.UploadImageAsync(input.Images, CurrentTenant.Id.ToString());
            entity.ThumbnailUrl = upload;
        }

        entity = await Repository.UpdateAsync(entity, autoSave: true);

        await SaveRelatedAsync(entity.Id, input.RelatedNewsIds);

        var dto = ObjectMapper.Map<News, AppNewsDto>(entity);
        dto.RelatedNewsIds = input.RelatedNewsIds ?? new List<Guid>();
        return dto;
    }
}