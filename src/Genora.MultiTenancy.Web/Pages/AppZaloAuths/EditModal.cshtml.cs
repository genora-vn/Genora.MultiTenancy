using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Genora.MultiTenancy.Web.Pages.AppZaloAuths;
public class EditModalModel : MultiTenancyPageModel
{
    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    [BindProperty]
    public CreateUpdateZaloAuthDto Auth { get; set; } = new();

    private readonly IAppZaloAuthAppService _service;

    public EditModalModel(IAppZaloAuthAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);

        Auth = new CreateUpdateZaloAuthDto
        {
            TenantId = null,
            AppId = dto.AppId,
            CodeChallenge = dto.CodeChallenge,
            CodeVerifier = dto.CodeVerifier,
            State = dto.State,
            AuthorizationCode = dto.AuthorizationCode,
            ExpireAuthorizationCodeTime = dto.ExpireAuthorizationCodeTime,
            AccessToken = dto.AccessToken,
            RefreshToken = dto.RefreshToken,
            ExpireTokenTime = dto.ExpireTokenTime,
            IsActive = dto.IsActive
        };
    }

    public async Task<IActionResult> OnPostAsync()
    {
        Auth.TenantId = null;
        await _service.UpdateAsync(Id, Auth);
        return NoContent();
    }
}