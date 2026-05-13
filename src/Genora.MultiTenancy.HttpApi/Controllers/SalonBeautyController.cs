using Volo.Abp;
using Microsoft.AspNetCore.Mvc;

namespace Genora.MultiTenancy.Controllers;

// Admin UI no longer calls this controller. Admin pages call Application Service JS proxies directly.
// Public / external Salon Beauty APIs are exposed by SalonBeautyMiniAppController.
[RemoteService(false)]
[Area("MultiTenancy")]
public class SalonBeautyController : MultiTenancyController
{
}
