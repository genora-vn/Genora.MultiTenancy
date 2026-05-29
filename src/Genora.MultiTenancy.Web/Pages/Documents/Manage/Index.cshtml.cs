using Genora.MultiTenancy.AppDtos.AppDocuments;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.Documents.Manage;

[Authorize(Policy = MultiTenancyPermissions.HostAppDocuments.Default)]
public class IndexModel : MultiTenancyPageModel
{
    private readonly IDocumentSectionAppService _sectionService;
    private readonly IDocumentPageAppService _pageService;

    public IndexModel(
        IDocumentSectionAppService sectionService,
        IDocumentPageAppService pageService)
    {
        _sectionService = sectionService;
        _pageService = pageService;
    }

    public void OnGet()
    {
    }

    public async Task<IActionResult> OnGetSectionsAsync()
    {
        var sections = await _sectionService.GetAllAsync();
        return new JsonResult(new
        {
            items = sections
                .OrderBy(x => x.DisplayOrder)
                .ThenBy(x => x.Name)
                .Select(x => new
                {
                    id = x.Id,
                    name = x.Name,
                    slug = x.Slug,
                    icon = x.Icon,
                    displayOrder = x.DisplayOrder,
                    status = (int)x.Status,
                    pageCount = x.PageCount
                })
        });
    }

    public async Task<IActionResult> OnGetPagesAsync(Guid? sectionId)
    {
        var input = new GetDocumentPageListInput
        {
            MaxResultCount = 200,
            SkipCount = 0,
            SectionId = sectionId
        };

        var pages = await _pageService.GetListAsync(input);

        return new JsonResult(new
        {
            items = pages.Items.Select(p => new
            {
                id = p.Id,
                title = p.Title,
                slug = p.Slug,
                sectionId = p.SectionId,
                sectionName = p.SectionName,
                sectionSlug = p.SectionSlug,
                displayOrder = p.DisplayOrder,
                status = (int)p.Status
            })
        });
    }

    public async Task<IActionResult> OnPostDeleteSectionAsync(Guid id)
    {
        await _sectionService.DeleteAsync(id);
        return new JsonResult(new { success = true });
    }

    public async Task<IActionResult> OnPostDeletePageAsync(Guid id)
    {
        await _pageService.DeleteAsync(id);
        return new JsonResult(new { success = true });
    }
}
