using Genora.MultiTenancy.AppDtos.AppDocuments;
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
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppDocuments;

[Authorize]
public class DocumentSectionAppService :
    CrudAppService<
        DocumentSection,
        DocumentSectionDto,
        Guid,
        GetDocumentSectionListInput,
        CreateUpdateDocumentSectionDto>,
    IDocumentSectionAppService
{
    private readonly IRepository<DocumentSection, Guid> _sectionRepo;
    private readonly IRepository<DocumentPage, Guid> _pageRepo;
    private readonly ICurrentTenant _currentTenant;

    public DocumentSectionAppService(
        IRepository<DocumentSection, Guid> repository,
        IRepository<DocumentPage, Guid> pageRepo,
        ICurrentTenant currentTenant)
        : base(repository)
    {
        _sectionRepo = repository;
        _pageRepo = pageRepo;
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

    public override async Task<PagedResultDto<DocumentSectionDto>> GetListAsync(GetDocumentSectionListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await _sectionRepo.GetQueryableAsync();
        var query = queryable;

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var f = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(f) || x.Slug.Contains(f));
        }

        if (input.Status.HasValue)
        {
            query = query.Where(x => x.Status == (byte)input.Status.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(DocumentSection.DisplayOrder) + " asc, " + nameof(DocumentSection.Name) + " asc"
            : input.Sorting;

        var sections = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));

        var sectionIds = sections.Select(s => s.Id).ToList();
        var pageQuery = await _pageRepo.GetQueryableAsync();
        var pageCounts = await AsyncExecuter.ToListAsync(
            pageQuery.Where(x => sectionIds.Contains(x.SectionId))
                     .GroupBy(x => x.SectionId)
                     .Select(g => new { SectionId = g.Key, Count = g.Count() })
        );

        var countMap = pageCounts.ToDictionary(x => x.SectionId, x => x.Count);

        var items = sections.Select(s => new DocumentSectionDto
        {
            Id = s.Id,
            Name = s.Name,
            Slug = s.Slug,
            Icon = s.Icon,
            DisplayOrder = s.DisplayOrder,
            FeatureName = s.FeatureName,
            TenantPermissionName = s.TenantPermissionName,
            HostPermissionName = s.HostPermissionName,
            Status = (DocumentStatus)s.Status,
            PageCount = countMap.TryGetValue(s.Id, out var c) ? c : 0,
            CreationTime = s.CreationTime,
            CreatorId = s.CreatorId,
            LastModificationTime = s.LastModificationTime,
            LastModifierId = s.LastModifierId
        }).ToList();

        return new PagedResultDto<DocumentSectionDto>(totalCount, items);
    }

    public async Task<List<DocumentSectionDto>> GetAllAsync()
    {
        await CheckGetListPolicyAsync();

        var queryable = await _sectionRepo.GetQueryableAsync();
        var sections = await AsyncExecuter.ToListAsync(
            queryable.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name));

        var sectionIds = sections.Select(s => s.Id).ToList();
        var pageQuery = await _pageRepo.GetQueryableAsync();
        var pageCounts = await AsyncExecuter.ToListAsync(
            pageQuery.Where(x => sectionIds.Contains(x.SectionId))
                     .GroupBy(x => x.SectionId)
                     .Select(g => new { SectionId = g.Key, Count = g.Count() })
        );
        var countMap = pageCounts.ToDictionary(x => x.SectionId, x => x.Count);

        return sections.Select(s => new DocumentSectionDto
        {
            Id = s.Id,
            Name = s.Name,
            Slug = s.Slug,
            Icon = s.Icon,
            DisplayOrder = s.DisplayOrder,
            FeatureName = s.FeatureName,
            TenantPermissionName = s.TenantPermissionName,
            HostPermissionName = s.HostPermissionName,
            Status = (DocumentStatus)s.Status,
            PageCount = countMap.TryGetValue(s.Id, out var c) ? c : 0
        }).ToList();
    }

    public override async Task<DocumentSectionDto> CreateAsync(CreateUpdateDocumentSectionDto input)
    {
        await CheckCreatePolicyAsync();

        var slug = !string.IsNullOrWhiteSpace(input.Slug)
            ? DocumentSlugifier.Slugify(input.Slug)
            : DocumentSlugifier.Slugify(input.Name);

        var allSlugs = (await AsyncExecuter.ToListAsync(
            (await _sectionRepo.GetQueryableAsync()).Select(x => x.Slug)
        )).ToHashSet(StringComparer.OrdinalIgnoreCase);

        slug = DocumentSlugifier.EnsureUnique(slug, s => allSlugs.Contains(s));

        var entity = new DocumentSection(GuidGenerator.Create(), input.Name, slug)
        {
            Icon = input.Icon,
            DisplayOrder = input.DisplayOrder,
            FeatureName = input.FeatureName,
            TenantPermissionName = input.TenantPermissionName,
            HostPermissionName = input.HostPermissionName,
            Status = (byte)input.Status
        };

        await _sectionRepo.InsertAsync(entity, autoSave: true);
        return await MapToSectionDtoAsync(entity);
    }

    public override async Task<DocumentSectionDto> UpdateAsync(Guid id, CreateUpdateDocumentSectionDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await _sectionRepo.GetAsync(id);
        entity.Name = input.Name;
        entity.Icon = input.Icon;
        entity.DisplayOrder = input.DisplayOrder;
        entity.FeatureName = input.FeatureName;
        entity.TenantPermissionName = input.TenantPermissionName;
        entity.HostPermissionName = input.HostPermissionName;
        entity.Status = (byte)input.Status;

        if (!string.IsNullOrWhiteSpace(input.Slug))
        {
            var newSlug = DocumentSlugifier.Slugify(input.Slug);
            if (!string.Equals(newSlug, entity.Slug, StringComparison.OrdinalIgnoreCase))
            {
                var taken = (await AsyncExecuter.ToListAsync(
                    (await _sectionRepo.GetQueryableAsync())
                        .Where(x => x.Id != id)
                        .Select(x => x.Slug)
                )).ToHashSet(StringComparer.OrdinalIgnoreCase);

                entity.Slug = DocumentSlugifier.EnsureUnique(newSlug, s => taken.Contains(s));
            }
        }

        await _sectionRepo.UpdateAsync(entity, autoSave: true);
        return await MapToSectionDtoAsync(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        // Cascade-delete pages of this section first.
        await _pageRepo.DeleteAsync(x => x.SectionId == id, autoSave: true);
        await _sectionRepo.DeleteAsync(id, autoSave: true);
    }

    private async Task<DocumentSectionDto> MapToSectionDtoAsync(DocumentSection s)
    {
        var pageQuery = await _pageRepo.GetQueryableAsync();
        var count = await AsyncExecuter.CountAsync(pageQuery.Where(x => x.SectionId == s.Id));

        return new DocumentSectionDto
        {
            Id = s.Id,
            Name = s.Name,
            Slug = s.Slug,
            Icon = s.Icon,
            DisplayOrder = s.DisplayOrder,
            FeatureName = s.FeatureName,
            TenantPermissionName = s.TenantPermissionName,
            HostPermissionName = s.HostPermissionName,
            Status = (DocumentStatus)s.Status,
            PageCount = count,
            CreationTime = s.CreationTime,
            CreatorId = s.CreatorId,
            LastModificationTime = s.LastModificationTime,
            LastModifierId = s.LastModifierId
        };
    }
}
