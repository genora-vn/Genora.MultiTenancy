using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
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
//[Authorize(MultiTenancyPermissions.HostAppZaloAuths.Default)]
public class HostZaloAuthController : MultiTenancyController
{
    private readonly IConfiguration _cfg;
    private readonly IRepository<ZaloAuth, Guid> _authRepo;
    private readonly IZaloOAuthClient _oauth;
    private readonly IZaloTokenProvider _tokenProvider;
    private readonly IRepository<ZaloLog, Guid> _logRepo;
    private readonly IStringEncryptionService _encrypt;

    public record TokenValueDto(string token);
    public record ActiveDto(DateTime? expireTokenTime, bool isExpired);

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
    /// API lấy lấy ra access_token và refresh_token để decode cho front end (tính năng copy)
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
        if (auth == null) throw new BusinessException("ZaloAuth:NotFound");

        var raw = kind?.ToLowerInvariant() switch
        {
            "access" => auth.AccessToken,
            "refresh" => auth.RefreshToken,
            _ => throw new BusinessException("ZaloAuth:InvalidTokenKind")
        };

        if (string.IsNullOrWhiteSpace(raw))
            throw new BusinessException("ZaloAuth:TokenEmpty");

        var plain = SecurityHelper.DecryptMaybe(raw, _encrypt)!;
        return Ok(new TokenValueDto(plain));
    }

    /// <summary>
    /// API xin quyền từ Zalo
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
                TenantId = null,
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
                action: Genora.MultiTenancy.Enums.ZaloLogActions.AUTHORIZE_URL,
                endpoint: endpoint,
                httpStatus: err == null ? 200 : 500,
                durationMs: sw.ElapsedMilliseconds,
                requestBody: null,
                responseBody: url,
                error: err,
                tenantId: null
            );
        }
    }

    /// <summary>
    /// API Zalo callback trả thông tin API (AccessToken, RefreshToken,...)
    /// </summary>
    /// <param name="code"></param>
    /// <param name="state"></param>
    /// <param name="oaId"></param>
    /// <param name="error"></param>
    /// <param name="errorCode"></param>
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
        if (!string.IsNullOrWhiteSpace(error) || !string.IsNullOrWhiteSpace(errorCode))
        {
            await ZaloLogHelper.InsertLogAsync(
                _logRepo,
                action: Genora.MultiTenancy.Enums.ZaloLogActions.EXCHANGE_CODE,
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

        var q = await _authRepo.GetQueryableAsync();
        var auth = q.FirstOrDefault(x => x.State == state && x.AppId == appId);
        if (auth == null) return BadRequest("Invalid state");

        if (auth.ExpireAuthorizationCodeTime.HasValue &&
            auth.ExpireAuthorizationCodeTime.Value < DateTime.UtcNow)
            return BadRequest("State expired");

        if (string.IsNullOrWhiteSpace(auth.CodeVerifier))
            return BadRequest("Missing code_verifier");

        auth.AuthorizationCode = code;
        auth.OaId = oaId;

        var token = await _oauth.ExchangeCodeAsync(appId, secret, code, auth.CodeVerifier!, redirectUri, oaId);

        // Lưu AccessToken và RefreshToken dưới dạng Encrypt
        auth.AccessToken = SecurityHelper.EncryptMaybe(token.AccessToken, _encrypt);
        auth.RefreshToken = SecurityHelper.EncryptMaybe(token.RefreshToken, _encrypt);
        auth.ExpireTokenTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
        auth.IsActive = true;

        await _authRepo.UpdateAsync(auth, autoSave: true);

        // Đảm bảo chỉ 1 token đang active
        await _tokenProvider.DeactivateOtherActivesAsync(auth.Id);

        return Redirect("/AppZaloAuths");
    }

    //private async Task DeactivateOtherActivesAsync(Guid keepId)
    //{
    //    var q = await _authRepo.GetQueryableAsync();
    //    var others = q.Where(x => x.IsActive && x.Id != keepId).ToList();
    //    foreach (var a in others) a.IsActive = false;
    //    foreach (var a in others) await _authRepo.UpdateAsync(a, autoSave: true);
    //}

    [HttpPost("refresh-now")]
    public async Task RefreshNowAsync()
    {
        await _tokenProvider.RefreshNowAsync();
    }

    [HttpGet("active")]
    public async Task<ActiveDto> GetActiveAsync()
    {
        // Clean active expired + đảm bảo 1 active non-expired
        var active = await ZaloAuthActiveNormalizer.EnsureSingleActiveNonExpiredAsync(_authRepo);

        if (active != null)
            return new ActiveDto(active.ExpireTokenTime, false);

        // Không còn active => lấy record mới nhất để biết "hết hạn lúc nào"
        var q = await _authRepo.GetQueryableAsync();
        var latest = q.OrderByDescending(x => x.CreationTime).FirstOrDefault();

        if (latest == null)
            return new ActiveDto(null, true);

        var isExpired = latest.ExpireTokenTime.HasValue && latest.ExpireTokenTime.Value <= DateTime.UtcNow;
        return new ActiveDto(latest.ExpireTokenTime, isExpired);
    }


    [HttpGet("active-status")]
    public async Task<object> GetActiveStatusAsync()
    {
        var a = await GetActiveAsync();
        return new { configured = a.expireTokenTime != null, expired = a.isExpired, expiresAtUtc = a.expireTokenTime };
    }
}