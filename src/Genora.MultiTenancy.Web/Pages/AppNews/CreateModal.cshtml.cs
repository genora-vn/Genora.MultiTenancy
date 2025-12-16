using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppNews;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppNewsDto News { get; set; }

    private readonly IAppNewsService _newsService;

    public CreateModalModel(IAppNewsService newsService)
    {
        _newsService = newsService;
    }

    public void OnGet()
    {
        News = new CreateUpdateAppNewsDto
        {
            Status = NewsStatus.Draft
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _newsService.CreateAsync(News);

        return NoContent();
    }
}