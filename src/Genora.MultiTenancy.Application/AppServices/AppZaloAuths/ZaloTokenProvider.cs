using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Microsoft.Extensions.Configuration;
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

    public ZaloTokenProvider(
        IRepository<ZaloAuth, Guid> authRepo,
        IConfiguration cfg,
        IZaloOAuthClient oauthClient,
        IStringEncryptionService encrypt)
    {
        _authRepo = authRepo;
        _cfg = cfg;
        _oauthClient = oauthClient;
        _encrypt = encrypt;
    }

    public async Task<string> GetAccessTokenAsync()
    {
        var auth = await GetActiveAuthAsync();

        // Decrypt
        var access = DecryptMaybe(auth.AccessToken);

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
            access = DecryptMaybe(auth.AccessToken);

            shouldRefresh = !auth.ExpireTokenTime.HasValue
                || auth.ExpireTokenTime.Value <= DateTime.UtcNow.AddSeconds(skewSeconds)
                || string.IsNullOrWhiteSpace(access);

            if (!shouldRefresh)
                return access!;

            await RefreshInternalAsync(auth);
            auth = await GetActiveAuthAsync();

            return DecryptMaybe(auth.AccessToken)!;
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

        var refresh = DecryptMaybe(auth.RefreshToken);
        if (string.IsNullOrWhiteSpace(refresh))
            throw new BusinessException("ZaloAuth:MissingRefreshToken");

        var token = await _oauthClient.RefreshTokenAsync(appId, secret, refresh, auth.OaId);

        auth.AccessToken = EncryptMaybe(token.AccessToken);
        auth.RefreshToken = EncryptMaybe(token.RefreshToken);
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

    private string? EncryptMaybe(string? plain)
        => string.IsNullOrWhiteSpace(plain) ? plain : _encrypt.Encrypt(plain);

    private string? DecryptMaybe(string? cipher)
    {
        if (string.IsNullOrWhiteSpace(cipher)) return cipher;
        try { return _encrypt.Decrypt(cipher); }
        catch { return cipher; }
    }
}