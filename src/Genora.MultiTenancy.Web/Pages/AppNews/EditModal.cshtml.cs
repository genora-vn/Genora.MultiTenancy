using Genora.MultiTenancy.AppDtos.AppNews;
using Genora.MultiTenancy.DomainModels.AppNews;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.Web.Pages.AppNews;

public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateAppNewsDto News { get; set; } = new();

    public Dictionary<Guid, string> RelatedNewsTitles { get; set; } = new();

    private readonly IAppNewsService _newsService;
    private readonly IRepository<News, Guid> _newsRepo;

    private const long MaxImageBytes = 20L * 1024 * 1024; // 20MB

    public EditModalModel(IAppNewsService newsService, IRepository<News, Guid> newsRepo)
    {
        _newsService = newsService;
        _newsRepo = newsRepo;
    }

    public async Task OnGetAsync()
    {
        var dto = await _newsService.GetAsync(Id);
        News = ObjectMapper.Map<AppNewsDto, CreateUpdateAppNewsDto>(dto);

        if (dto.RelatedNewsIds != null && dto.RelatedNewsIds.Count > 0)
        {
            var ids = dto.RelatedNewsIds.Distinct().ToList();
            var list = await _newsRepo.GetListAsync(x => ids.Contains(x.Id));
            RelatedNewsTitles = list.ToDictionary(x => x.Id, x => x.Title);
        }

        News.IsUploadImage = false;
        News.Images = null;
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (News == null) News = new CreateUpdateAppNewsDto();

        var current = await _newsService.GetAsync(Id);

        if (News.IsUploadImage)
        {
            var len = News.Images?.ContentLength ?? 0;

            if (len <= 0) ModelState.AddModelError("News.Images", "Vui lòng chọn ảnh để upload trước khi lưu.");
            else if (len > MaxImageBytes) ModelState.AddModelError("News.Images", "Ảnh vượt quá 20MB. Vui lòng chọn ảnh nhỏ hơn.");

            var ct = News.Images?.ContentType ?? "";
            if (len > 0 && !ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
                ModelState.AddModelError("News.Images", "File không phải ảnh hợp lệ.");
        }
        else
        {
            if (string.IsNullOrWhiteSpace(News.ThumbnailUrl))
                News.ThumbnailUrl = current.ThumbnailUrl;

            if (string.IsNullOrWhiteSpace(News.ThumbnailUrl))
                ModelState.AddModelError("News.ThumbnailUrl", "Vui lòng nhập URL ảnh đại diện.");

            News.Images = null;
        }

        if (!ModelState.IsValid) return Page();

        await _newsService.UpdateAsync(Id, News);
        return NoContent();
    }
}
