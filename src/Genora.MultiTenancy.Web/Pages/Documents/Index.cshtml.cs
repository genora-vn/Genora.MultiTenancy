using Genora.MultiTenancy.AppDtos.AppDocuments;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.Documents;

[Authorize]
public class IndexModel : MultiTenancyPageModel
{
    private readonly IDocumentReaderAppService _reader;

    [BindProperty(SupportsGet = true, Name = "slug")]
    public string? Slug { get; set; }

    public string? SectionSlug { get; set; }
    public string? PageSlug { get; set; }

    public DocumentTreeDto Tree { get; set; } = new();
    public DocumentReadDto? Document { get; set; }
    public bool DocumentNotFound { get; set; }

    public IndexModel(IDocumentReaderAppService reader)
    {
        _reader = reader;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        Tree = await _reader.GetVisibleTreeAsync();

        ParseSlug();

        // /Documents → landing: load first available page as welcome.
        if (string.IsNullOrWhiteSpace(SectionSlug))
        {
            Document = await _reader.GetFirstAvailablePageAsync();
            return Page();
        }

        // /Documents/{section} → redirect to first page in that section.
        if (string.IsNullOrWhiteSpace(PageSlug))
        {
            var section = Tree.Sections.Find(s =>
                string.Equals(s.SectionSlug, SectionSlug, System.StringComparison.OrdinalIgnoreCase));

            if (section != null && section.Pages.Count > 0)
            {
                return Redirect($"/Documents/{section.SectionSlug}/{section.Pages[0].Slug}");
            }

            DocumentNotFound = true;
            return Page();
        }

        // /Documents/{section}/{page}
        Document = await _reader.GetPageBySlugAsync(SectionSlug, PageSlug);
        DocumentNotFound = Document == null;
        return Page();
    }

    private void ParseSlug()
    {
        var raw = (Slug ?? string.Empty).Trim('/');
        if (raw.Length == 0)
        {
            SectionSlug = null;
            PageSlug = null;
            return;
        }

        var parts = raw.Split('/', System.StringSplitOptions.RemoveEmptyEntries);
        SectionSlug = parts.Length > 0 ? parts[0] : null;
        PageSlug = parts.Length > 1 ? parts[1] : null;
    }
}
