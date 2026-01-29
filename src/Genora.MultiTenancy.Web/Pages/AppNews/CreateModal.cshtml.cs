using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.Enums;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppNews;

public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppNewsDto News { get; set; } = new();

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
            Status = NewsStatus.Draft,
            IsUploadImage = false,
            Images = null
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (News == null) News = new CreateUpdateAppNewsDto();

        if (News.IsUploadImage)
        {
            var len = News.Images?.ContentLength ?? 0;

            if (len <= 0)
            {
                ModelState.AddModelError("News.Images", "Vui lòng chọn ảnh để upload trước khi lưu.");
            }
            else if (len > MaxImageBytes)
            {
                ModelState.AddModelError("News.Images", "Ảnh vượt quá 20MB. Vui lòng chọn ảnh nhỏ hơn.");
            }

            var ct = News.Images?.ContentType ?? "";
            if (len > 0 && !ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            {
                ModelState.AddModelError("News.Images", "File không phải ảnh hợp lệ.");
            }
        }
        else
        {
            if (string.IsNullOrWhiteSpace(News.ThumbnailUrl))
            {
                ModelState.AddModelError("News.ThumbnailUrl", "Vui lòng nhập URL ảnh đại diện.");
            }
            News.Images = null;
        }

        if (!ModelState.IsValid)
        {
            return Page();
        }

        await _newsService.CreateAsync(News);
        return NoContent();
    }
}
