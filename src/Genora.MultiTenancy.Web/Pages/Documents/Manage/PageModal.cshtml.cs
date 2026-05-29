using Genora.MultiTenancy.AppDtos.AppDocuments;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Content;

namespace Genora.MultiTenancy.Web.Pages.Documents.Manage;

[Authorize(Policy = MultiTenancyPermissions.HostAppDocuments.Default)]
public class PageModalModel : MultiTenancyPageModel
{
    private readonly IDocumentSectionAppService _sectionService;
    private readonly IDocumentPageAppService _pageService;
    private readonly IDocumentMetadataAppService _metaService;

    [BindProperty(SupportsGet = true)]
    public Guid? Id { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid? SectionId { get; set; }

    [BindProperty]
    public CreateUpdateDocumentPageDto Page { get; set; } = new();

    public bool IsEdit => Id.HasValue && Id.Value != Guid.Empty;

    public List<SelectListItem> StatusItems { get; set; } = new();
    public List<SelectListItem> SectionItems { get; set; } = new();
    public List<SelectListItem> FeatureItems { get; set; } = new();
    public List<SelectListItem> TenantPermissionItems { get; set; } = new();
    public List<SelectListItem> HostPermissionItems { get; set; } = new();

    public PageModalModel(
        IDocumentSectionAppService sectionService,
        IDocumentPageAppService pageService,
        IDocumentMetadataAppService metaService)
    {
        _sectionService = sectionService;
        _pageService = pageService;
        _metaService = metaService;
    }

    public async Task OnGetAsync()
    {
        await LoadLookupsAsync();

        if (IsEdit)
        {
            var dto = await _pageService.GetAsync(Id!.Value);
            Page = new CreateUpdateDocumentPageDto
            {
                SectionId = dto.SectionId,
                Title = dto.Title,
                Slug = dto.Slug,
                ContentHtml = string.Empty, // lazy-loaded via OnGetContentAsync
                DisplayOrder = dto.DisplayOrder,
                Status = dto.Status,
                FeatureName = dto.FeatureName,
                TenantPermissionName = dto.TenantPermissionName,
                HostPermissionName = dto.HostPermissionName
            };
        }
        else
        {
            Page.Status = DocumentStatus.Published;
            if (SectionId.HasValue) Page.SectionId = SectionId.Value;
            else if (SectionItems.Count > 0) Page.SectionId = Guid.Parse(SectionItems[0].Value);
        }
    }

    public async Task<IActionResult> OnGetContentAsync()
    {
        if (!Id.HasValue) return new JsonResult(new { contentHtml = string.Empty });
        var dto = await _pageService.GetContentAsync(Id.Value);
        return new JsonResult(new { contentHtml = dto?.ContentHtml ?? string.Empty });
    }

    public async Task<IActionResult> OnPostUploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return new JsonResult(new { url = (string?)null });
        }

        await using var stream = file.OpenReadStream();
        var content = new RemoteStreamContent(stream, file.FileName, file.ContentType, file.Length);

        var url = await _pageService.UploadImageAsync(content);
        return new JsonResult(new { url });
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadLookupsAsync();
            return base.Page();
        }

        if (IsEdit)
        {
            await _pageService.UpdateAsync(Id!.Value, Page);
        }
        else
        {
            await _pageService.CreateAsync(Page);
        }

        return NoContent();
    }

    private async Task LoadLookupsAsync()
    {
        StatusItems = Enum.GetValues<DocumentStatus>()
            .Select(v => new SelectListItem(L[$"DocumentStatus:{v}"].Value, ((int)v).ToString()))
            .ToList();

        var sections = await _sectionService.GetAllAsync();
        SectionItems = sections
            .OrderBy(x => x.DisplayOrder).ThenBy(x => x.Name)
            .Select(s => new SelectListItem(s.Name, s.Id.ToString()))
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
