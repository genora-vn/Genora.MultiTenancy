using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.AppImages;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Volo.Abp.Authorization;
using Volo.Abp.Content;
using Volo.Abp.Features;
namespace Genora.MultiTenancy.Web.Pages.Hlg;
[Authorize]
public class UploadModel : MultiTenancyPageModel
{
    private readonly IManageImageService _images;
    private readonly IAuthorizationService _authorization;
    private readonly IFeatureChecker _features;
    public UploadModel(IManageImageService images, IAuthorizationService authorization, IFeatureChecker features) { _images=images; _authorization=authorization; _features=features; }
    public async Task<IActionResult> OnPostAsync(IFormFile file)
    {
        if (CurrentTenant.IsAvailable && !await _features.IsEnabledAsync(AppHlgFeatures.Management)) throw new AbpAuthorizationException();
        var granted=false;
        foreach (var group in new[] { "Knowledge", "Games", "Rewards", "Content" })
            foreach (var action in new[] { ".Create", ".Edit" })
                granted |= await _authorization.IsGrantedAsync("MultiTenancy."+(CurrentTenant.IsAvailable?"AppHlg":"HostAppHlg")+group+action);
        if (!granted) throw new AbpAuthorizationException();
        if (file == null || file.Length <= 0 || file.Length > 5 * 1024 * 1024) return BadRequest();
        await using var stream=file.OpenReadStream();
        var url=await _images.UploadImageAsync(new RemoteStreamContent(stream,file.FileName,file.ContentType,file.Length),CurrentTenant.Id?.ToString() ?? "host","hlg");
        return new JsonResult(new { url });
    }
}
