using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Security.Encryption;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloTokenProvider : IZaloTokenProvider
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private readonly IRepository<ZaloAuth, Guid> _authRepo;
    private readonly IConfiguration _cfg;
    private readonly IZaloOAuthClient _oauthClient;
    private readonly IStringEncryptionService _encrypt;
    private readonly ILogger<ZaloTokenProvider> _logger;

    public ZaloTokenProvider(
        IRepository<ZaloAuth, Guid> authRepo,
        IConfiguration cfg,
        IZaloOAuthClient oauthClient,
        IStringEncryptionService encrypt,
        ILogger<ZaloTokenProvider> logger)
    {
        _authRepo = authRepo;
        _cfg = cfg;
        _oauthClient = oauthClient;
        _encrypt = encrypt;
        _logger = logger;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var auth = await GetActiveAuthAsync();

        // Decrypt
        var access = SecurityHelper.DecryptMaybe(auth.AccessToken, _encrypt);

        var skewSeconds = _cfg.GetValue<int>("Zalo:TokenRefreshSkewSeconds", 60);
        var shouldRefresh = !auth.ExpireTokenTime.HasValue
            || auth.ExpireTokenTime.Value <= DateTime.UtcNow.AddSeconds(skewSeconds)
            || string.IsNullOrWhiteSpace(access);

        if (!shouldRefresh)
            return access!;

        await _lock.WaitAsync();
        try
        {
            // Check lại sau khi đã khóa
            auth = await GetActiveAuthAsync();
            access = SecurityHelper.DecryptMaybe(auth.AccessToken, _encrypt);

            shouldRefresh = !auth.ExpireTokenTime.HasValue
                || auth.ExpireTokenTime.Value <= DateTime.UtcNow.AddSeconds(skewSeconds)
                || string.IsNullOrWhiteSpace(access);

            if (!shouldRefresh)
                return access!;

            await RefreshInternalAsync(auth);
            auth = await GetActiveAuthAsync();

            return SecurityHelper.DecryptMaybe(auth.AccessToken, _encrypt)!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RefreshNowAsync()
    {
        var auth = await GetActiveAuthAsync();

        await _lock.WaitAsync();
        try
        {
            await RefreshInternalAsync(auth);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task RefreshInternalAsync(ZaloAuth auth)
    {
        var appId = _cfg["Zalo:AppId"]!;
        var secret = _cfg["Zalo:AppSecret"]!;

        var refresh = SecurityHelper.DecryptMaybe(auth.RefreshToken, _encrypt);

        _logger.LogInformation("Zalo refresh token len={Len} head={H} tail={T}",
    refresh.Length, refresh.Substring(0, 4), refresh.Substring(refresh.Length - 4));

        if (string.IsNullOrWhiteSpace(refresh))
            throw new BusinessException("ZaloAuth:MissingRefreshToken");

        var token = await _oauthClient.RefreshTokenAsync(appId, secret, refresh, auth.OaId);

        auth.AccessToken = SecurityHelper.EncryptMaybe(token.AccessToken, _encrypt);
        auth.RefreshToken = SecurityHelper.EncryptMaybe(token.RefreshToken, _encrypt);
        auth.ExpireTokenTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn);

        await _authRepo.UpdateAsync(auth, autoSave: true);
    }

    private async Task<ZaloAuth> GetActiveAuthAsync()
    {
        var q = await _authRepo.GetQueryableAsync();
        var auth = q.Where(x => x.IsActive).OrderByDescending(x => x.CreationTime).FirstOrDefault();

        if (auth == null)
            throw new BusinessException("ZaloAuth:NotConfigured");

        return auth;
    }
}