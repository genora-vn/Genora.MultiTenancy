using Genora.MultiTenancy.AppDtos.AppNews;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;

namespace Genora.MultiTenancy.Web.Pages.AppNews;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateAppNewsDto News { get; set; }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    private readonly IAppNewsService _newsService;
    private const long MaxImageBytes = 20L * 1024 * 1024; // 20MB

    public EditModalModel(IAppNewsService newsService)
    {
        _newsService = newsService;
    }

    public async Task OnGetAsync()
    {
        var dto = await _newsService.GetAsync(Id);
        News = ObjectMapper.Map<AppNewsDto, CreateUpdateAppNewsDto>(dto);
        //News = new CreateUpdateAppNewsDto
        //{
        //    Title = dto.Title,
        //    ContentHtml = dto.ContentHtml,
        //    ThumbnailUrl = dto.ThumbnailUrl,
        //    PublishedAt = dto.PublishedAt,
        //    Status = dto.Status,
        //    DisplayOrder = dto.DisplayOrder,
        //    ShortDescription = dto.ShortDescription,
        //    IsActive = 
        //};
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

        await _newsService.UpdateAsync(Id, News);
        return NoContent();
    }
}