using System.Threading.Tasks;
using Genora.MultiTenancy.Features.AppHoaLinhFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Web.Pages.HoaLinh.GiftReceipts;

[Authorize]
public class IndexModel : PageModel
{
    private readonly ICurrentTenant _tenant;
    private readonly IAuthorizationService _auth;
    private readonly IFeatureChecker _features;
    public bool CanExport { get; private set; }
    public IndexModel(ICurrentTenant tenant, IAuthorizationService auth, IFeatureChecker features)
    { _tenant = tenant; _auth = auth; _features = features; }

    public async Task<IActionResult> OnGetAsync()
    {
        var root = _tenant.IsAvailable ? MultiTenancyPermissions.AppHlGiftReceipts.Default : MultiTenancyPermissions.HostAppHlGiftReceipts.Default;
        if (_tenant.IsAvailable && (!await _features.IsEnabledAsync(AppHoaLinhFeatures.Management)
            || !await _features.IsEnabledAsync(AppHoaLinhFeatures.GiftReceipts))) return Forbid();
        if (!(await _auth.AuthorizeAsync(User, root)).Succeeded) return Forbid();
        CanExport = (await _auth.AuthorizeAsync(User, root + ".Export")).Succeeded;
        return Page();
    }
}
