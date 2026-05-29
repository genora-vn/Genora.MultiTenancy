using Genora.MultiTenancy.AppDtos.AppDocuments;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.DomainModels.AppDocuments;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppDocuments;

[Authorize]
public class DocumentPageAppService :
    CrudAppService<
        DocumentPage,
        DocumentPageDto,
        Guid,
        GetDocumentPageListInput,
        CreateUpdateDocumentPageDto>,
    IDocumentPageAppService
{
    private readonly IRepository<DocumentPage, Guid> _pageRepo;
    private readonly IRepository<DocumentSection, Guid> _sectionRepo;
    private readonly IManageImageService _imageService;
    private readonly ICurrentTenant _currentTenant;

    private const long MaxImageBytes = 10L * 1024 * 1024;

    public DocumentPageAppService(
        IRepository<DocumentPage, Guid> repository,
        IRepository<DocumentSection, Guid> sectionRepo,
        IManageImageService imageService,
        ICurrentTenant currentTenant)
        : base(repository)
    {
        _pageRepo = repository;
        _sectionRepo = sectionRepo;
        _imageService = imageService;
        _currentTenant = currentTenant;

        GetPolicyName = MultiTenancyPermissions.AppDocuments.Default;
        GetListPolicyName = MultiTenancyPermissions.AppDocuments.Default;
        CreatePolicyName = MultiTenancyPermissions.HostAppDocuments.Create;
        UpdatePolicyName = MultiTenancyPermissions.HostAppDocuments.Edit;
        DeletePolicyName = MultiTenancyPermissions.HostAppDocuments.Delete;
    }

    private string P(string tenantPermission)
    {
        if (_currentTenant.IsAvailable) return tenantPermission;

        const string tenantRoot = MultiTenancyPermissions.AppDocuments.Default;
        const string hostRoot = MultiTenancyPermissions.HostAppDocuments.Default;

        if (tenantPermission.StartsWith(tenantRoot))
            return hostRoot + tenantPermission.Substring(tenantRoot.Length);

        return tenantPermission;
    }

    protected override async Task CheckGetPolicyAsync()
        => await AuthorizationService.CheckAsync(P(GetPolicyName!));

    protected override async Task CheckGetListPolicyAsync()
        => await AuthorizationService.CheckAsync(P(GetListPolicyName!));

    public override async Task<PagedResultDto<DocumentPageDto>> GetListAsync(GetDocumentPageListInput input)
    {
        await CheckGetListPolicyAsync();

        var pageQ = await _pageRepo.GetQueryableAsync();
        var sectionQ = await _sectionRepo.GetQueryableAsync();

        var query = from p in pageQ
                    join s in sectionQ on p.SectionId equals s.Id
                    select new { Page = p, Section = s };

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.Page.Title.Contains(f) || x.Page.Slug.Contains(f));
        }

        if (input.SectionId.HasValue)
        {
            query = query.Where(x => x.Page.SectionId == input.SectionId.Value);
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Page.Status == (byte)input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sortingRaw = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(DocumentPage.DisplayOrder) + " asc, " + nameof(DocumentPage.Title) + " asc"
            : input.Sorting;

        // Map sorting on Page.* into anonymous projection
        var sorting = sortingRaw.Replace("Title", "Page.Title", StringComparison.OrdinalIgnoreCase)
                                .Replace("DisplayOrder", "Page.DisplayOrder", StringComparison.OrdinalIgnoreCase);
        // Defensive fallback if user passes anonymous-friendly string
        try { query = query.OrderBy(sorting); }
        catch { query = query.OrderBy("Page.DisplayOrder asc, Page.Title asc"); }

        var rows = await AsyncExecuter.ToListAsync(
            query.Skip(input.SkipCount).Take(input.MaxResultCount));

        var items = rows.Select(r => new DocumentPageDto
        {
            Id = r.Page.Id,
            SectionId = r.Page.SectionId,
            SectionName = r.Section.Name,
            SectionSlug = r.Section.Slug,
            Title = r.Page.Title,
            Slug = r.Page.Slug,
            DisplayOrder = r.Page.DisplayOrder,
            Status = (DocumentStatus)r.Page.Status,
            FeatureName = r.Page.FeatureName,
            TenantPermissionName = r.Page.TenantPermissionName,
            HostPermissionName = r.Page.HostPermissionName,
            CreationTime = r.Page.CreationTime,
            CreatorId = r.Page.CreatorId,
            LastModificationTime = r.Page.LastModificationTime,
            LastModifierId = r.Page.LastModifierId
        }).ToList();

        return new PagedResultDto<DocumentPageDto>(totalCount, items);
    }

    public override async Task<DocumentPageDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var entity = await _pageRepo.GetAsync(id);
        var section = await _sectionRepo.GetAsync(entity.SectionId);

        return new DocumentPageDto
        {
            Id = entity.Id,
            SectionId = entity.SectionId,
            SectionName = section.Name,
            SectionSlug = section.Slug,
            Title = entity.Title,
            Slug = entity.Slug,
            DisplayOrder = entity.DisplayOrder,
            Status = (DocumentStatus)entity.Status,
            FeatureName = entity.FeatureName,
            TenantPermissionName = entity.TenantPermissionName,
            HostPermissionName = entity.HostPermissionName,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId
        };
    }

    public async Task<DocumentPageContentDto> GetContentAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var entity = await _pageRepo.GetAsync(id);
        return new DocumentPageContentDto
        {
            Id = entity.Id,
            Title = entity.Title,
            ContentHtml = entity.ContentHtml ?? string.Empty
        };
    }

    public override async Task<DocumentPageDto> CreateAsync(CreateUpdateDocumentPageDto input)
    {
        await CheckCreatePolicyAsync();

        // Ensure section exists
        await _sectionRepo.GetAsync(input.SectionId);

        var baseSlug = !string.IsNullOrWhiteSpace(input.Slug)
            ? DocumentSlugifier.Slugify(input.Slug)
            : DocumentSlugifier.Slugify(input.Title);

        var taken = await GetTakenSlugsAsync(input.SectionId, excludeId: null);
        var slug = DocumentSlugifier.EnsureUnique(baseSlug, s => taken.Contains(s));

        var entity = new DocumentPage(GuidGenerator.Create(), input.SectionId, input.Title, slug)
        {
            ContentHtml = input.ContentHtml ?? string.Empty,
            DisplayOrder = input.DisplayOrder,
            Status = (byte)input.Status,
            FeatureName = input.FeatureName,
            TenantPermissionName = input.TenantPermissionName,
            HostPermissionName = input.HostPermissionName
        };

        await _pageRepo.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public override async Task<DocumentPageDto> UpdateAsync(Guid id, CreateUpdateDocumentPageDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await _pageRepo.GetAsync(id);

        // Section change supported
        if (entity.SectionId != input.SectionId)
        {
            await _sectionRepo.GetAsync(input.SectionId);
            entity.SectionId = input.SectionId;
        }

        entity.Title = input.Title;
        entity.ContentHtml = input.ContentHtml ?? string.Empty;
        entity.DisplayOrder = input.DisplayOrder;
        entity.Status = (byte)input.Status;
        entity.FeatureName = input.FeatureName;
        entity.TenantPermissionName = input.TenantPermissionName;
        entity.HostPermissionName = input.HostPermissionName;

        if (!string.IsNullOrWhiteSpace(input.Slug))
        {
            var newSlug = DocumentSlugifier.Slugify(input.Slug);
            if (!string.Equals(newSlug, entity.Slug, StringComparison.OrdinalIgnoreCase))
            {
                var taken = await GetTakenSlugsAsync(entity.SectionId, excludeId: id);
                entity.Slug = DocumentSlugifier.EnsureUnique(newSlug, s => taken.Contains(s));
            }
        }

        await _pageRepo.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public async Task<string> UploadImageAsync(IRemoteStreamContent file)
    {
        await AuthorizationService.CheckAsync(MultiTenancyPermissions.HostAppDocuments.Edit);

        if (file == null || (file.ContentLength ?? 0) <= 0)
        {
            throw new BusinessException("Documents:UploadImage:Empty", "File rỗng.");
        }

        if ((file.ContentLength ?? 0) > MaxImageBytes)
        {
            throw new BusinessException("Documents:UploadImage:TooLarge",
                $"Ảnh vượt quá {MaxImageBytes / (1024 * 1024)}MB.");
        }

        var contentType = file.ContentType ?? string.Empty;
        if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
        {
            throw new BusinessException("Documents:UploadImage:NotImage", "File không phải ảnh hợp lệ.");
        }

        var url = await _imageService.UploadImageAsync(file, "host", "documents");
        return url;
    }

    private async Task<HashSet<string>> GetTakenSlugsAsync(Guid sectionId, Guid? excludeId)
    {
        var q = await _pageRepo.GetQueryableAsync();
        var query = q.Where(x => x.SectionId == sectionId);
        if (excludeId.HasValue) query = query.Where(x => x.Id != excludeId.Value);

        var slugs = await AsyncExecuter.ToListAsync(query.Select(x => x.Slug));
        return slugs.ToHashSet(StringComparer.OrdinalIgnoreCase);
    }
}
