using Genora.MultiTenancy.AppDtos.AppDocuments;
using Genora.MultiTenancy.DomainModels.AppDocuments;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization.Permissions;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppDocuments;

[Authorize]
public class DocumentReaderAppService : ApplicationService, IDocumentReaderAppService
{
    private readonly IRepository<DocumentSection, Guid> _sectionRepo;
    private readonly IRepository<DocumentPage, Guid> _pageRepo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IFeatureChecker _featureChecker;
    private readonly IPermissionChecker _permissionChecker;

    public DocumentReaderAppService(
        IRepository<DocumentSection, Guid> sectionRepo,
        IRepository<DocumentPage, Guid> pageRepo,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IPermissionChecker permissionChecker)
    {
        _sectionRepo = sectionRepo;
        _pageRepo = pageRepo;
        _currentTenant = currentTenant;
        _featureChecker = featureChecker;
        _permissionChecker = permissionChecker;
    }

    public async Task<DocumentTreeDto> GetVisibleTreeAsync()
    {
        // Documents are stored in the HOST database (entities aren't IMultiTenant). When a tenant
        // has its own connection string, the repository would query the tenant DB instead — switch
        // to host scope to always hit the shared documentation tables.
        List<DocumentSection> sections;
        List<DocumentPage> pages;
        using (_currentTenant.Change(null))
        {
            var sectionQ = await _sectionRepo.GetQueryableAsync();
            var pageQ = await _pageRepo.GetQueryableAsync();

            sections = await AsyncExecuter.ToListAsync(
                sectionQ.Where(x => x.Status == (byte)DocumentStatus.Published)
                        .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name));

            pages = await AsyncExecuter.ToListAsync(
                pageQ.Where(x => x.Status == (byte)DocumentStatus.Published)
                     .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Title));
        }

        var tree = new DocumentTreeDto();

        foreach (var s in sections)
        {
            if (!await IsVisibleAsync(s.FeatureName, s.TenantPermissionName, s.HostPermissionName))
                continue;

            var node = new DocumentTreeNodeDto
            {
                SectionId = s.Id,
                SectionName = s.Name,
                SectionSlug = s.Slug,
                Icon = s.Icon,
                DisplayOrder = s.DisplayOrder
            };

            foreach (var p in pages.Where(x => x.SectionId == s.Id))
            {
                // Inherit gates: page check overrides section if set
                var featName = !string.IsNullOrWhiteSpace(p.FeatureName) ? p.FeatureName : s.FeatureName;
                var tenantPerm = !string.IsNullOrWhiteSpace(p.TenantPermissionName) ? p.TenantPermissionName : s.TenantPermissionName;
                var hostPerm = !string.IsNullOrWhiteSpace(p.HostPermissionName) ? p.HostPermissionName : s.HostPermissionName;

                if (!await IsVisibleAsync(featName, tenantPerm, hostPerm))
                    continue;

                node.Pages.Add(new DocumentTreePageDto
                {
                    PageId = p.Id,
                    Title = p.Title,
                    Slug = p.Slug,
                    DisplayOrder = p.DisplayOrder
                });
            }

            // Hide empty sections
            if (node.Pages.Count == 0) continue;

            tree.Sections.Add(node);
        }

        return tree;
    }

    public async Task<DocumentReadDto?> GetPageBySlugAsync(string sectionSlug, string pageSlug)
    {
        if (string.IsNullOrWhiteSpace(sectionSlug) || string.IsNullOrWhiteSpace(pageSlug)) return null;

        using (_currentTenant.Change(null))
        {
            var sectionQ = await _sectionRepo.GetQueryableAsync();
            var pageQ = await _pageRepo.GetQueryableAsync();

            var section = await AsyncExecuter.FirstOrDefaultAsync(
                sectionQ.Where(x => x.Slug == sectionSlug && x.Status == (byte)DocumentStatus.Published));
            if (section == null) return null;

            if (!await IsVisibleAsync(section.FeatureName, section.TenantPermissionName, section.HostPermissionName))
                return null;

            var page = await AsyncExecuter.FirstOrDefaultAsync(
                pageQ.Where(x => x.SectionId == section.Id && x.Slug == pageSlug
                              && x.Status == (byte)DocumentStatus.Published));
            if (page == null) return null;

            var featName = !string.IsNullOrWhiteSpace(page.FeatureName) ? page.FeatureName : section.FeatureName;
            var tenantPerm = !string.IsNullOrWhiteSpace(page.TenantPermissionName) ? page.TenantPermissionName : section.TenantPermissionName;
            var hostPerm = !string.IsNullOrWhiteSpace(page.HostPermissionName) ? page.HostPermissionName : section.HostPermissionName;

            if (!await IsVisibleAsync(featName, tenantPerm, hostPerm)) return null;

            return new DocumentReadDto
            {
                PageId = page.Id,
                SectionId = section.Id,
                SectionName = section.Name,
                SectionSlug = section.Slug,
                Title = page.Title,
                Slug = page.Slug,
                ContentHtml = page.ContentHtml ?? string.Empty,
                LastModificationTime = page.LastModificationTime ?? page.CreationTime
            };
        }
    }

    public async Task<DocumentReadDto?> GetFirstAvailablePageAsync()
    {
        var tree = await GetVisibleTreeAsync();
        var firstSection = tree.Sections.FirstOrDefault();
        var firstPage = firstSection?.Pages.FirstOrDefault();
        if (firstSection == null || firstPage == null) return null;

        return await GetPageBySlugAsync(firstSection.SectionSlug, firstPage.Slug);
    }

    private async Task<bool> IsVisibleAsync(string? featureName, string? tenantPermission, string? hostPermission)
    {
        // Feature gate (only meaningful if a tenant is in scope; for host, features are always considered enabled)
        if (!string.IsNullOrWhiteSpace(featureName) && _currentTenant.IsAvailable)
        {
            try
            {
                if (!await _featureChecker.IsEnabledAsync(featureName))
                    return false;
            }
            catch
            {
                // Unknown feature key — fail open so docs aren't accidentally hidden by a typo.
            }
        }

        // Permission gate
        if (_currentTenant.IsAvailable)
        {
            if (!string.IsNullOrWhiteSpace(tenantPermission))
            {
                if (!await _permissionChecker.IsGrantedAsync(tenantPermission))
                    return false;
            }
        }
        else
        {
            if (!string.IsNullOrWhiteSpace(hostPermission))
            {
                if (!await _permissionChecker.IsGrantedAsync(hostPermission))
                    return false;
            }
        }

        return true;
    }
}
