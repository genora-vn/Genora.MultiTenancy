using System;
using System.IO;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Features;
namespace Genora.MultiTenancy.Web.Pages.Hlg;
[Authorize]
[RequestSizeLimit(100 * 1024 * 1024)]
[RequestFormLimits(MultipartBodyLengthLimit = 100 * 1024 * 1024)]
public class UploadModel : MultiTenancyPageModel
{
    private readonly IManageImageService _images;
    private readonly IAuthorizationService _authorization;
    private readonly IFeatureChecker _features;
    private readonly IWebHostEnvironment _environment;
    private const long MaxVideoLength = 100 * 1024 * 1024;
    public UploadModel(IManageImageService images, IAuthorizationService authorization, IFeatureChecker features, IWebHostEnvironment environment) { _images=images; _authorization=authorization; _features=features; _environment=environment; }
    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        await EnsureAuthorizedAsync();
        if (file == null || file.Length <= 0 || file.Length > 5 * 1024 * 1024) return BadRequest();
        await using var stream=file.OpenReadStream();
        var url=await _images.UploadImageAsync(new RemoteStreamContent(stream,file.FileName,file.ContentType,file.Length),CurrentTenant.Id?.ToString() ?? "host","hlg");
        return new JsonResult(new { url });
    }

    public async Task<IActionResult> OnPostVideoAsync(IFormFile file)
    {
        await EnsureAuthorizedAsync();
        if (file == null || file.Length <= 0 || file.Length > MaxVideoLength) return BadRequest();

        var extension = Path.GetExtension(file.FileName)?.ToLowerInvariant() ?? "";
        if (extension is not (".mp4" or ".webm" or ".ogv" or ".ogg" or ".mov") ||
            !await HasValidVideoSignatureAsync(file, extension)) return BadRequest();

        var tenantId = CurrentTenant.Id?.ToString() ?? "host";
        var uploadRoot = Path.Combine(_environment.WebRootPath ?? Path.Combine(_environment.ContentRootPath, "wwwroot"), "uploads", "hlg", tenantId, "videos");
        Directory.CreateDirectory(uploadRoot);
        var fileName = Guid.NewGuid().ToString("N") + extension;
        var filePath = Path.Combine(uploadRoot, fileName);
        await using (var source = file.OpenReadStream())
        await using (var destination = new FileStream(filePath, FileMode.CreateNew, FileAccess.Write, FileShare.None, 81920, true))
            await source.CopyToAsync(destination);

        return new JsonResult(new { url = $"/uploads/hlg/{tenantId}/videos/{fileName}" });
    }

    private async Task EnsureAuthorizedAsync()
    {
        if (CurrentTenant.IsAvailable && !await _features.IsEnabledAsync(AppHlgFeatures.Management)) throw new AbpAuthorizationException();
        var granted=false;
        foreach (var group in new[] { "Knowledge", "Games", "Rewards", "Content" })
            foreach (var action in new[] { ".Create", ".Edit" })
                granted |= await _authorization.IsGrantedAsync("MultiTenancy."+(CurrentTenant.IsAvailable?"AppHlg":"HostAppHlg")+group+action);
        if (!granted) throw new AbpAuthorizationException();
    }

    private static async Task<bool> HasValidVideoSignatureAsync(IFormFile file, string extension)
    {
        var header = new byte[12];
        await using var stream = file.OpenReadStream();
        var read = await stream.ReadAsync(header.AsMemory(0, header.Length));
        if (extension is ".mp4" or ".mov")
            return read >= 8 && header[4] == (byte)'f' && header[5] == (byte)'t' && header[6] == (byte)'y' && header[7] == (byte)'p';
        if (extension == ".webm")
            return read >= 4 && header[0] == 0x1A && header[1] == 0x45 && header[2] == 0xDF && header[3] == 0xA3;
        return read >= 4 && header[0] == (byte)'O' && header[1] == (byte)'g' && header[2] == (byte)'g' && header[3] == (byte)'S';
    }
}
