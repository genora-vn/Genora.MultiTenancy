using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Volo.Abp.Content;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Web.Pages.HoaLinh.BlouseConfig;

[Authorize]
[RequestSizeLimit(10 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 10 * 1024 * 1024)]
public class IndexModel : PageModel
{
    private readonly IManageImageService _images;
    private readonly IConfiguration _configuration;
    private readonly ICurrentTenant _currentTenant;

    public IndexModel(
        IManageImageService images,
        IConfiguration configuration,
        ICurrentTenant currentTenant)
    {
        _images = images;
        _configuration = configuration;
        _currentTenant = currentTenant;
    }

    public void OnGet() { }

    /// <summary>
    /// POST /HoaLinh/BlouseConfig?handler=UploadImage — upload ảnh banner chương trình.
    /// Lưu path tương đối (/uploads/hl-blouse/...) vào DB, trả về cả path + full URL (App:AppUrl).
    /// </summary>
    public async Task<IActionResult> OnPostUploadImageAsync(IFormFile file)
    {
        if (file == null || file.Length <= 0)
            return BadRequest(new { message = "Thiếu file ảnh." });
        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Ảnh vượt quá 5MB." });

        await using var stream = file.OpenReadStream();
        var relative = await _images.UploadImageAsync(
            new RemoteStreamContent(stream, file.FileName, file.ContentType, file.Length),
            _currentTenant.Id?.ToString() ?? "host",
            "hl-blouse");

        return new JsonResult(new
        {
            path = relative,
            url = ImageHelper.NormalizeThumb(_configuration, relative)
        });
    }
}
