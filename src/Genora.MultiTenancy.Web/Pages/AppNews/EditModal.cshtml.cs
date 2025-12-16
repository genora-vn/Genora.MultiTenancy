using Genora.MultiTenancy.AppDtos.AppNews;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppNews;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppNewsDto News { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    private readonly IAppNewsService _newsService;

    public EditModalModel(IAppNewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _newsService.GetAsync(Id);
        News = new CreateUpdateAppNewsDto
        {
            Title = dto.Title,
            ContentHtml = dto.ContentHtml,
            ThumbnailUrl = dto.ThumbnailUrl,
            PublishedAt = dto.PublishedAt,
            Status = dto.Status,
            DisplayOrder = dto.DisplayOrder
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _newsService.UpdateAsync(Id, News);
        return NoContent();
    }
}