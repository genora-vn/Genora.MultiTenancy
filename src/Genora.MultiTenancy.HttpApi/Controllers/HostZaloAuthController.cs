using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;

namespace Genora.MultiTenancy.Controllers;

[Area("MultiTenancy")]
[Route("api/host/zalo-auth")]
[Authorize(MultiTenancyPermissions.HostAppZaloAuths.Default)]
public class HostZaloAuthController : MultiTenancyController
{
    private readonly IConfiguration _cfg;
    private readonly IRepository<ZaloAuth, Guid> _authRepo;
    private readonly IZaloOAuthClient _oauth;
    private readonly IZaloTokenProvider _tokenProvider;
    private readonly IRepository<ZaloLog, Guid> _logRepo;

    public HostZaloAuthController(
        IConfiguration cfg,
        IRepository<ZaloAuth, Guid> authRepo,
        IZaloOAuthClient oauth,
        IZaloTokenProvider tokenProvider,
        IRepository<ZaloLog, Guid> logRepo)
    {
        _cfg = cfg;
        _authRepo = authRepo;
        _oauth = oauth;
        _tokenProvider = tokenProvider;
        _logRepo = logRepo;
    }

    /// <summary>
    /// API tạo URL để lấy Authorization Code của Zalo
    /// </summary>
    /// <returns></returns>
    [HttpGet("authorize-url")]
    public async Task<object> GetAuthorizeUrlAsync()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        string? url = null;
        string? err = null;

        var endpoint = "https://oauth.zaloapp.com/v4/oa/permission";

        try
        {
            var appId = _cfg["Zalo:AppId"]!;
            var redirectUri = _cfg["Zalo:RedirectUri"]!;
            var method = _cfg.GetValue<string>("Zalo:CodeChallengeMethod", "S256");

            var verifier = PkceUtil.CreateCodeVerifier();
            var challenge = method == "S256"
                ? PkceUtil.CreateCodeChallengeS256(verifier)
                : verifier;

            var state = Guid.NewGuid().ToString("N");
            var ttl = _cfg.GetValue<int>("Zalo:AuthorizationCodeTtlMinutes", 5);

            var auth = new ZaloAuth()
            {
                AppId = appId,
                CodeVerifier = verifier,
                CodeChallenge = challenge,
                State = state,
                ExpireAuthorizationCodeTime = DateTime.UtcNow.AddMinutes(ttl),
                IsActive = true
            };

            await _authRepo.InsertAsync(auth, autoSave: true);

            url = endpoint
                + $"?app_id={Uri.EscapeDataString(appId)}"
                + $"&redirect_uri={Uri.EscapeDataString(redirectUri)}"
                + $"&code_challenge={Uri.EscapeDataString(challenge)}"
                + $"&code_challenge_method={Uri.EscapeDataString(method)}"
                + $"&state={Uri.EscapeDataString(state)}";

            return new { authorizeUrl = url, state };
        }
        catch (Exception ex)
        {
            err = ex.ToString();
            throw;
        }
        finally
        {
            sw.Stop();
            await ZaloLogHelper.InsertLogAsync(
                _logRepo,
                action: "AUTHORIZE_URL",
                endpoint: endpoint,
                httpStatus: err == null ? 200 : 500,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: null,
                responseBody: url,
                error: err,
                tenantId: null // host
            );
        }
    }

    /// <summary>
    /// API callback nhận dữ liệu trả về sau khi xin quyền để lấy Access token và Refresh token và lưu vào DB
    /// </summary>
    /// <param name="code">Authorization Code Zalo trả về</param>
    /// <param name="state">State Zalo trả về</param>
    /// <returns></returns>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<IActionResult> CallbackAsync([FromQuery] string code, [FromQuery] string state)
    {
        // Lấy cấu hình
        var appId = _cfg["Zalo:AppId"]!;
        var secret = _cfg["Zalo:AppSecret"]!;
        var redirectUri = _cfg["Zalo:RedirectUri"]!;

        // Tìm record theo state
        var q = await _authRepo.GetQueryableAsync();
        var auth = q.FirstOrDefault(x => x.State == state && x.AppId == appId);

        if (auth == null)
            return BadRequest("Invalid state");

        if (auth.ExpireAuthorizationCodeTime.HasValue && auth.ExpireAuthorizationCodeTime.Value < DateTime.UtcNow)
            return BadRequest("State expired");

        if (string.IsNullOrWhiteSpace(auth.CodeVerifier))
            return BadRequest("Missing code_verifier");

        auth.AuthorizationCode = code;

        // Call service exchange code -> tokens
        var token = await _oauth.ExchangeCodeAsync(appId, secret, code, auth.CodeVerifier, redirectUri);

        // Token provider encrypt trong provider, ở lưu plain trước rồi provider tự decrypt fallback.
        auth.AccessToken = token.AccessToken;
        auth.RefreshToken = token.RefreshToken;
        auth.ExpireTokenTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        await _authRepo.UpdateAsync(auth, autoSave: true);

        // Redirect về trang danh sách
        return Redirect("/ZaloAuths");
    }

    /// <summary>
    /// API Refresh lấy lại AcessToken dựa vào Refresh token
    /// </summary>
    /// <returns></returns>
    [HttpPost("refresh-now")]
    public async Task RefreshNowAsync()
    {
        await _tokenProvider.RefreshNowAsync();
    }
}
