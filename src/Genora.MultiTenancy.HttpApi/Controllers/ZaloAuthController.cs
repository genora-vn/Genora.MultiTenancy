using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppServices.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace Genora.MultiTenancy.Controllers;

[Area("MultiTenancy")]
[Route("api/zalo-auth")]
public class ZaloAuthController : MultiTenancyController
{
    private readonly IConfiguration _cfg;
    private readonly IRepository<ZaloAuth, Guid> _authRepo;
    private readonly IZaloTokenProvider _tokenProvider;
    private readonly IZaloLogWriter _logWriter;
    private readonly IStringEncryptionService _encrypt;
    private readonly ICurrentTenant _currentTenant;

    public record TokenValueDto(string token);
    public record ActiveDto(DateTime? expireTokenTime, bool isExpired);

    public ZaloAuthController(
        IConfiguration cfg,
        IRepository<ZaloAuth, Guid> authRepo,
        IZaloTokenProvider tokenProvider,
        IZaloLogWriter logWriter,
        IStringEncryptionService encrypt,
        ICurrentTenant currentTenant)
    {
        _cfg = cfg;
        _authRepo = authRepo;
        _tokenProvider = tokenProvider;
        _logWriter = logWriter;
        _encrypt = encrypt;
        _currentTenant = currentTenant;
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

        var tenantId = _currentTenant.Id;

        try
        {
            var appId = _cfg["Zalo:AppId"]!;
            var redirectUri = _cfg["Zalo:RedirectUri"]!;
            var method = _cfg.GetValue("Zalo:CodeChallengeMethod", "S256");

            var verifier = PkceUtil.CreateCodeVerifier();
            var challenge = method == "S256"
                ? PkceUtil.CreateCodeChallengeS256(verifier)
                : verifier;

            var state = tenantId + "_" + Guid.NewGuid().ToString("N");

            await _authRepo.InsertAsync(new ZaloAuth
            {
                TenantId = tenantId,
                AppId = appId,
                CodeVerifier = verifier,
                CodeChallenge = challenge,
                State = state,
                ExpireAuthorizationCodeTime = DateTime.UtcNow.AddMinutes(5),
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
                tenantId
            );
        }
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
