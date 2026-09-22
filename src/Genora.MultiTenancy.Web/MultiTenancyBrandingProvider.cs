using Microsoft.Extensions.Localization;
using Genora.MultiTenancy.Localization;
using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Ui.Branding;

namespace Genora.MultiTenancy.Web;

[Dependency(ReplaceServices = true)]
public class MultiTenancyBrandingProvider : DefaultBrandingProvider
{
    private readonly ICurrentTenant _currentTenant;
    private readonly IStringLocalizer<MultiTenancyResource> _localizer;

    public MultiTenancyBrandingProvider(
        ICurrentTenant currentTenant,
        IStringLocalizer<MultiTenancyResource> localizer)
    {
        _currentTenant = currentTenant;
        _localizer = localizer;
    }

    public override string AppName => _currentTenant.Name ?? _localizer["AppName"];
}