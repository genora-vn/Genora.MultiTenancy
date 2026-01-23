using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppNews;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppNewsDto News { get; set; }

    private readonly IAppNewsService _newsService;
    private const long MaxImageBytes = 20L * 1024 * 1024; // 20MB

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
        if (News.IsUploadImage && News.Images != null)
        {
            if (News.IsUploadImage && News.Images != null)
            {
                var len = News.Images.ContentLength ?? 0;

                if (len <= 0)
                {
                    ModelState.AddModelError("News.Images", "Vui lòng chọn ảnh.");
                }
                else if (len > MaxImageBytes)
                {
                    ModelState.AddModelError("News.Images", "Ảnh vượt quá 20MB. Vui lòng chọn ảnh nhỏ hơn.");
                }

                var contentType = News.Images.ContentType ?? "";
                if (!contentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError("News.Images", "File không phải ảnh hợp lệ.");
                }
            }

            var ct = News.Images.ContentType ?? "";
            if (!ct.StartsWith("image/"))
            {
                ModelState.AddModelError("News.Images", "File không phải ảnh hợp lệ.");
            }
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _newsService.CreateAsync(News);

        return NoContent();
    }
}