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

    public string? AccessTokenMasked { get; set; }
    public string? RefreshTokenMasked { get; set; }

    private readonly IAppZaloAuthAppService _service;

    public EditModalModel(IAppZaloAuthAppService service)
    {
        _service = service;
    }

    public async Task OnGetAsync()
    {
        var dto = await _service.GetAsync(Id);

        AccessTokenMasked = dto.AccessTokenMasked;
        RefreshTokenMasked = dto.RefreshTokenMasked;

        Auth = new CreateUpdateZaloAuthDto
        {
            TenantId = null,
            AppId = dto.AppId,
            OaId = dto.OaId,
            CodeChallenge = dto.CodeChallenge,
            CodeVerifier = dto.CodeVerifier,
            State = dto.State,
            AuthorizationCode = dto.AuthorizationCode,
            ExpireAuthorizationCodeTime = dto.ExpireAuthorizationCodeTime,

            // ✅ token mới để trống
            AccessToken = null,
            RefreshToken = null,

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
