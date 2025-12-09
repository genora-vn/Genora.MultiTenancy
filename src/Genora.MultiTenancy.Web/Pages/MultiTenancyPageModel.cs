using Genora.MultiTenancy.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace Genora.MultiTenancy.Web.Pages;

public abstract class MultiTenancyPageModel : AbpPageModel
{
    protected MultiTenancyPageModel()
    {
        LocalizationResourceType = typeof(MultiTenancyResource);
    }
}
