using System;
using System.Threading.Tasks;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Volo.Abp.Authorization;
using Volo.Abp.Features;
namespace Genora.MultiTenancy.Web.Pages.Hlg;
[Authorize]
public abstract class HlgAdminPageModel : MultiTenancyPageModel
{
    protected abstract string PermissionGroup { get; }
    protected virtual string ActionSuffix => "";
    public Task<bool> GrantedAsync(string suffix) => HttpContext.RequestServices.GetRequiredService<IAuthorizationService>()
        .IsGrantedAsync("MultiTenancy." + (CurrentTenant.IsAvailable ? "AppHlg" : "HostAppHlg") + PermissionGroup + suffix);
    public override async Task OnPageHandlerExecutionAsync(PageHandlerExecutingContext context, PageHandlerExecutionDelegate next)
    {
        var features = HttpContext.RequestServices.GetRequiredService<IFeatureChecker>();
        if (CurrentTenant.IsAvailable && !await features.IsEnabledAsync(AppHlgFeatures.Management)) throw new AbpAuthorizationException();
        if (!await GrantedAsync("") || !await GrantedAsync(ActionSuffix)) throw new AbpAuthorizationException();
        await next();
    }
}
