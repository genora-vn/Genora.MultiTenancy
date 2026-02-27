using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Settings;

namespace Genora.MultiTenancy.Controllers;

[Area("MultiTenancy")]
[Route("api/host/zalo-auth")]
public class HostZaloAuthController : MultiTenancyController
{
    private readonly IRepository<ZaloAuth, Guid> _authRepo;
    private readonly IZaloOAuthClient _oauth;
    private readonly IZaloTokenProvider _tokenProvider;
    private readonly IZaloLogWriter _logWriter;
    private readonly IStringEncryptionService _encrypt;
    private readonly ICurrentTenant _currentTenant;
    private readonly IConfiguration _cfg;
    private readonly IZaloRuntimeConfigProvider _zaloCfg;

    public record TokenValueDto(string token);
    public record ActiveDto(DateTime? expireTokenTime, bool isExpired);

    public HostZaloAuthController(
        IRepository<ZaloAuth, Guid> authRepo,
        IZaloOAuthClient oauth,
        IZaloTokenProvider tokenProvider,
        IZaloLogWriter logWriter,
        IStringEncryptionService encrypt,
        ICurrentTenant currentTenant,
        ISettingProvider settingProvider,
        IConfiguration cfg,
        IZaloRuntimeConfigProvider zaloCfg)
    {
        _authRepo = authRepo;
        _oauth = oauth;
        _tokenProvider = tokenProvider;
        _logWriter = logWriter;
        _encrypt = encrypt;
        _currentTenant = currentTenant;
        _cfg = cfg;
        _zaloCfg = zaloCfg;
    }

    [HttpGet("{id}/token")]
    public async Task<ActionResult<TokenValueDto>> GetPlainTokenAsync(Guid id, [FromQuery] string kind)
    {
        var auth = (await _authRepo.GetQueryableAsync()).FirstOrDefault(x => x.Id == id);
        if (auth == null) throw new BusinessException("ZaloAuth:NotFound");

        var raw = kind?.ToLowerInvariant() switch
        {
            "access" => auth.AccessToken,
            "refresh" => auth.RefreshToken,
            _ => throw new BusinessException("ZaloAuth:InvalidTokenKind")
        };

        if (string.IsNullOrWhiteSpace(raw))
            throw new BusinessException("ZaloAuth:TokenEmpty");

        return Ok(new TokenValueDto(SecurityHelper.DecryptMaybe(raw, _encrypt)!));
    }

    [HttpGet("authorize-url")]
    public async Task<object> GetAuthorizeUrlAsync()
    {
        var sw = System.Diagnostics.Stopwatch.StartNew();
        string? url = null;
        string? err = null;
        const string endpoint = "https://oauth.zaloapp.com/v4/oa/permission";

        try
        {
            var config = await _zaloCfg.GetAsync();
            var appId = config.AppId;
            var redirectUri = config.RedirectUri;
            var method = _cfg.GetValue("Zalo:CodeChallengeMethod", "S256");

            var verifier = PkceUtil.CreateCodeVerifier();
            var challenge = method == "S256"
                ? PkceUtil.CreateCodeChallengeS256(verifier)
                : verifier;

            var state = "host_" + Guid.NewGuid().ToString("N");
            var ttl = _cfg.GetValue("Zalo:AuthorizationCodeTtlMinutes", 5);

            await _authRepo.InsertAsync(new ZaloAuth
            {
                TenantId = null,
                AppId = appId,
                CodeVerifier = verifier,
                CodeChallenge = challenge,
                State = state,
                ExpireAuthorizationCodeTime = DateTime.UtcNow.AddMinutes(ttl),
                IsActive = true
            }, true);

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
            await _logWriter.WriteAsync(
                ZaloLogActions.AUTHORIZE_URL,
                endpoint,
                err == null ? 200 : 500,
                sw.ElapsedMilliseconds,
                null,
                url,
                err,
                tenantId: null
            );
        }
    }

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
            await _logWriter.WriteAsync(
                ZaloLogActions.EXCHANGE_CODE,
                "CALLBACK",
                400,
                0,
                null,
                $"error={error}; error_code={errorCode}; state={state}",
                "Zalo callback error",
                null
            );
            return Redirect("/AppZaloAuths?zaloError=1");
        }

        if (string.IsNullOrWhiteSpace(code) || string.IsNullOrWhiteSpace(state))
            return BadRequest("Missing code/state");

        Guid? tenantId = null;

        if (!state.StartsWith("host_", StringComparison.OrdinalIgnoreCase))
        {
            var head = state.Split('_')[0];
            if (Guid.TryParse(head, out var tid))
                tenantId = tid;
        }

        using (_currentTenant.Change(tenantId))
        {
            var config = await _zaloCfg.GetAsync();
            var appId = config.AppId;
            var secret = config.AppSecret;
            var redirectUri = config.RedirectUri;
            var auth = (await _authRepo.GetQueryableAsync())
                .FirstOrDefault(x => x.State == state && x.AppId == appId);

            if (auth == null) return BadRequest("Invalid state");

            var token = await _oauth.ExchangeCodeAsync(
                appId, secret, code, auth.CodeVerifier!, redirectUri, oaId);

            auth.OaId = oaId;
            auth.AccessToken = SecurityHelper.EncryptMaybe(token.AccessToken, _encrypt);
            auth.RefreshToken = SecurityHelper.EncryptMaybe(token.RefreshToken, _encrypt);
            auth.ExpireTokenTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn);
            auth.IsActive = true;

            await _authRepo.UpdateAsync(auth, true);
            await _tokenProvider.DeactivateOtherActivesAsync(auth.Id);
        }

        return Redirect("/AppZaloAuths");
    }

    [HttpPost("refresh-now")]
    public Task RefreshNowAsync() => _tokenProvider.RefreshNowAsync();

    [HttpGet("active")]
    public async Task<ActiveDto> GetActiveAsync()
    {
        var active = await ZaloAuthActiveNormalizer.EnsureSingleActiveNonExpiredAsync(_authRepo);
        if (active != null) return new ActiveDto(active.ExpireTokenTime, false);

        var latest = (await _authRepo.GetQueryableAsync())
            .OrderByDescending(x => x.CreationTime)
            .FirstOrDefault();

        return latest == null
            ? new ActiveDto(null, true)
            : new ActiveDto(latest.ExpireTokenTime,
                latest.ExpireTokenTime <= DateTime.UtcNow);
    }
}
