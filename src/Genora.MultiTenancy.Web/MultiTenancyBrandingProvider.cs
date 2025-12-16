using Volo.Abp.DependencyInjection;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Ui.Branding;

namespace Genora.MultiTenancy.Web;

[Dependency(ReplaceServices = true)]
public class MultiTenancyBrandingProvider : DefaultBrandingProvider
{
    //private IStringLocalizer<MultiTenancyResource> _localizer;

    //public MultiTenancyBrandingProvider(IStringLocalizer<MultiTenancyResource> localizer)
    //{
    //    _localizer = localizer;
    //}

    //public override string AppName => _localizer["AppName"];
    private readonly ICurrentTenant _currentTenant;
    public override string AppName => _currentTenant.Name ?? "CustomHost";

    public MultiTenancyBrandingProvider(ICurrentTenant currentTenant)
    {
        _currentTenant = currentTenant;
    }
}