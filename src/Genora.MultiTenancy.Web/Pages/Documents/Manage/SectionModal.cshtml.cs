using Genora.MultiTenancy.AppDtos.AppDocuments;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Genora.MultiTenancy.Web.Pages.Documents.Manage;

[Authorize(Policy = MultiTenancyPermissions.HostAppDocuments.Default)]
public class SectionModalModel : MultiTenancyPageModel
{
    private readonly IDocumentSectionAppService _sectionService;
    private readonly IDocumentMetadataAppService _metaService;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty]
    public CreateUpdateDocumentSectionDto Section { get; set; } = new();

    public bool IsEdit => Id.HasValue && Id.Value != Guid.Empty;

    public List<SelectListItem> StatusItems { get; set; } = new();
    public List<SelectListItem> FeatureItems { get; set; } = new();
    public List<SelectListItem> TenantPermissionItems { get; set; } = new();
    public List<SelectListItem> HostPermissionItems { get; set; } = new();

    public SectionModalModel(
        IDocumentSectionAppService sectionService,
        IDocumentMetadataAppService metaService)
    {
        _sectionService = sectionService;
        _metaService = metaService;
    }

    public async Task OnGetAsync()
    {
        await LoadLookupsAsync();

        if (IsEdit)
        {
            var dto = await _sectionService.GetAsync(Id!.Value);
            Section = new CreateUpdateDocumentSectionDto
            {
                Name = dto.Name,
                Slug = dto.Slug,
                Icon = dto.Icon,
                DisplayOrder = dto.DisplayOrder,
                Status = dto.Status,
                FeatureName = dto.FeatureName,
                TenantPermissionName = dto.TenantPermissionName,
                HostPermissionName = dto.HostPermissionName
            };
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return Page();
        }

        if (IsEdit)
        {
            await _sectionService.UpdateAsync(Id!.Value, Section);
        }
        else
        {
            await _sectionService.CreateAsync(Section);
        }

        return NoContent();
    }

    private async Task LoadLookupsAsync()
    {
        StatusItems = Enum.GetValues<DocumentStatus>()
            .Select(v => new SelectListItem(L[$"DocumentStatus:{v}"].Value, ((int)v).ToString()))
            .ToList();

        var features = await _metaService.GetFeatureLookupAsync();
        FeatureItems = features
            .Select(f => new SelectListItem($"{f.Name} ({f.Value})", f.Value))
            .ToList();

        var tenantPerms = await _metaService.GetTenantPermissionLookupAsync();
        TenantPermissionItems = tenantPerms
            .Select(p => new SelectListItem(p.Value, p.Value))
            .ToList();

        var hostPerms = await _metaService.GetHostPermissionLookupAsync();
        HostPermissionItems = hostPerms
            .Select(p => new SelectListItem(p.Value, p.Value))
            .ToList();
    }
}
