using Genora.MultiTenancy.Localization;
using Volo.Abp.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Controllers;

/* Inherit your controllers from this class.
 */
public abstract class MultiTenancyController : AbpControllerBase
{
    protected MultiTenancyController()
    {
        LocalizationResource = typeof(MultiTenancyResource);
    }
}
