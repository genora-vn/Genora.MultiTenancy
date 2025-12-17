using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.Web.Pages.AppZaloAuths;
public class CreateModalModel : MultiTenancyPageModel
{
    [BindProperty]
    public CreateUpdateZaloAuthDto Auth { get; set; } = new();

    private readonly IAppZaloAuthAppService _service;
    private readonly ICurrentTenant _currentTenant;

    public CreateModalModel(IAppZaloAuthAppService service, ICurrentTenant currentTenant)
    {
        _service = service;
        _currentTenant = currentTenant;
    }

    public void OnGet()
    {
        Auth.IsActive = true;
        Auth.TenantId = null; // host-only
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Auth.TenantId = null; // host-only
        await _service.CreateAsync(Auth);
        return NoContent();
    }
}