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
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;

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
    private readonly IStringEncryptionService _encrypt;
    public record TokenValueDto(string token);

    public HostZaloAuthController(
        IConfiguration cfg,
        IRepository<ZaloAuth, Guid> authRepo,
        IZaloOAuthClient oauth,
        IZaloTokenProvider tokenProvider,
        IRepository<ZaloLog, Guid> logRepo,
        IStringEncryptionService encrypt)
    {
        _cfg = cfg;
        _authRepo = authRepo;
        _oauth = oauth;
        _tokenProvider = tokenProvider;
        _logRepo = logRepo;
        _encrypt = encrypt;
    }


    /// <summary>
    /// API lấy ra token
    /// </summary>
    /// <param name="id"></param>
    /// <param name="kind"></param>
    /// <returns></returns>
    /// <exception cref="BusinessException"></exception>
    [HttpGet("{id}/token")]
    public async Task<ActionResult<TokenValueDto>> GetPlainTokenAsync(Guid id, [FromQuery] string kind)
    {
        var q = await _authRepo.GetQueryableAsync();
        var auth = q.FirstOrDefault(x => x.Id == id);
        if (auth == null)
            throw new BusinessException("ZaloAuth:NotFound");

        var raw = kind?.ToLowerInvariant() switch
        {
            "access" => auth.AccessToken,
            "refresh" => auth.RefreshToken,
            _ => throw new BusinessException("ZaloAuth:InvalidTokenKind")
        };

        if (string.IsNullOrWhiteSpace(raw))
            throw new BusinessException("ZaloAuth:TokenEmpty");

        var plain = SecurityHelper.DecryptMaybe(raw, _encrypt)!;

        // trả JSON để abp.ajax không bị parsererror
        return Ok(new TokenValueDto(plain));
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
    public async Task<IActionResult> CallbackAsync(
    [FromQuery] string? code,
    [FromQuery] string? state,
    [FromQuery(Name = "oa_id")] string? oaId,
    [FromQuery] string? error,
    [FromQuery(Name = "error_code")] string? errorCode)
    {
        // Nếu user từ chối / Zalo trả lỗi
        if (!string.IsNullOrWhiteSpace(error) || !string.IsNullOrWhiteSpace(errorCode))
        {
            await ZaloLogHelper.InsertLogAsync(
                _logRepo,
                action: "EXCHANGE_CODE",
                endpoint: "CALLBACK",
                httpStatus: 400,
                durationMs: 0,
                requestBody: null,
                responseBody: $"error={error}; error_code={errorCode}; state={state}; oa_id={oaId}",
                error: "Zalo callback returned error",
                tenantId: null
            );

            return Redirect("/AppZaloAuths?zaloError=1");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            return BadRequest("Missing code/state");

        var appId = _cfg["Zalo:AppId"]!;
        var secret = _cfg["Zalo:AppSecret"]!;
        var redirectUri = _cfg["Zalo:RedirectUri"]!;

        // tìm record theo state
        var q = await _authRepo.GetQueryableAsync();

        // Match theo appId + state (oaId có thể null ở record tạo trước callback)
        var auth = q.FirstOrDefault(x => x.State == state && x.AppId == appId);

        if (auth == null)
            return BadRequest("Invalid state");

        if (auth.ExpireAuthorizationCodeTime.HasValue &&
            auth.ExpireAuthorizationCodeTime.Value < DateTime.UtcNow)
            return BadRequest("State expired");

        if (string.IsNullOrWhiteSpace(auth.CodeVerifier))
            return BadRequest("Missing code_verifier");

        // Lưu code + oa_id
        auth.AuthorizationCode = code;
        auth.OaId = oaId;

        // Đổi code -> token (TRUYỀN oa_id)
        var token = await _oauth.ExchangeCodeAsync(
            appId, secret, code, auth.CodeVerifier!, redirectUri, oaId
        );

        auth.AccessToken = token.AccessToken;
        auth.RefreshToken = token.RefreshToken;
        auth.ExpireTokenTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        await _authRepo.UpdateAsync(auth, autoSave: true);

        return Redirect("/AppZaloAuths");
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
