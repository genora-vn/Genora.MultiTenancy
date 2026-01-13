using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
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
        var auth = await GetBestAuthForUseOrRefreshAsync();

        var access = SecurityHelper.DecryptMaybe(auth.AccessToken, _encrypt);

        var skewSeconds = _cfg.GetValue<int>("Zalo:TokenRefreshSkewSeconds", 60);
        var shouldRefresh =
            string.IsNullOrWhiteSpace(access) ||
            !auth.ExpireTokenTime.HasValue ||
            auth.ExpireTokenTime.Value <= DateTime.UtcNow.AddSeconds(skewSeconds);

        if (!shouldRefresh && auth.IsActive) // chỉ return nếu record đang active
            return access!;

        await _lock.WaitAsync();
        try
        {
            auth = await GetBestAuthForUseOrRefreshAsync();
            access = SecurityHelper.DecryptMaybe(auth.AccessToken, _encrypt);

            shouldRefresh =
                string.IsNullOrWhiteSpace(access) ||
                !auth.ExpireTokenTime.HasValue ||
                auth.ExpireTokenTime.Value <= DateTime.UtcNow.AddSeconds(skewSeconds) ||
                !auth.IsActive; // Nếu không active thì cũng refresh để tạo active mới

            if (!shouldRefresh && auth.IsActive)
                return access!;

            var newAuth = await RefreshAndRotateAsync(auth);
            return SecurityHelper.DecryptMaybe(newAuth.AccessToken, _encrypt)!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RefreshNowAsync()
    {
        var auth = await GetBestAuthForUseOrRefreshAsync();

        await _lock.WaitAsync();
        try
        {
            await RefreshAndRotateAsync(auth);
        }
        finally
        {
            _lock.Release();
        }
    }

    private async Task<ZaloAuth> RefreshAndRotateAsync(ZaloAuth current)
    {
        var appId = _cfg["Zalo:AppId"]!;
        var secret = _cfg["Zalo:AppSecret"]!;

        var refreshPlain = SecurityHelper.DecryptMaybe(current.RefreshToken, _encrypt);
        if (string.IsNullOrWhiteSpace(refreshPlain))
            throw new BusinessException("ZaloAuth:MissingRefreshToken");

        var token = await _oauthClient.RefreshTokenAsync(appId, secret, refreshPlain, current.OaId);

        current.IsActive = false;
        await _authRepo.UpdateAsync(current, autoSave: true);

        var newAuth = new ZaloAuth
        {
            TenantId = null,
            AppId = current.AppId,
            OaId = current.OaId,

            CodeChallenge = current.CodeChallenge,
            CodeVerifier = current.CodeVerifier,
            State = current.State,
            AuthorizationCode = current.AuthorizationCode,
            ExpireAuthorizationCodeTime = current.ExpireAuthorizationCodeTime,

            AccessToken = SecurityHelper.EncryptMaybe(token.AccessToken, _encrypt),
            RefreshToken = SecurityHelper.EncryptMaybe(token.RefreshToken, _encrypt),
            ExpireTokenTime = DateTime.UtcNow.AddSeconds(token.ExpiresIn),

            IsActive = true
        };

        await _authRepo.InsertAsync(newAuth, autoSave: true);

        await ZaloAuthActiveNormalizer.SetActiveOnlyAsync(_authRepo, newAuth.Id);
        await CleanupInactiveHistoryAsync();

        return newAuth;
    }

    public async Task DeactivateOtherActivesAsync(Guid keepId)
    {
        var q = await _authRepo.GetQueryableAsync();
        var actives = q.Where(x => x.IsActive && x.Id != keepId).ToList();
        if (actives.Count == 0) return;

        foreach (var a in actives) a.IsActive = false;
        foreach (var a in actives) await _authRepo.UpdateAsync(a, autoSave: true);
    }

    private async Task CleanupInactiveHistoryAsync()
    {
        var maxInactive = _cfg.GetValue<int>("Zalo:TokenHistoryMaxInactive", 50);
        if (maxInactive <= 0) return;

        var q = await _authRepo.GetQueryableAsync();

        // chỉ dọn host token
        var oldIds = q.Where(x => x.TenantId == null && !x.IsActive)
                      .OrderByDescending(x => x.CreationTime)
                      .Skip(maxInactive)
                      .Select(x => x.Id)
                      .ToList();

        foreach (var id in oldIds)
        {
            await _authRepo.DeleteAsync(id, autoSave: true);
        }
    }

    private async Task<ZaloAuth> GetBestAuthForUseOrRefreshAsync()
    {
        var q = await _authRepo.GetQueryableAsync();

        // 1) Nếu có active non-expired => ưu tiên dùng
        var active = q.Where(x => x.IsActive)
                      .OrderByDescending(x => x.CreationTime)
                      .FirstOrDefault();

        if (active != null)
        {
            // Active expired => auto inactive
            if (active.ExpireTokenTime.HasValue && active.ExpireTokenTime.Value <= DateTime.UtcNow)
            {
                active.IsActive = false;
                await _authRepo.UpdateAsync(active, autoSave: true);
            }
            else
            {
                return active;
            }
        }

        // 2) Không có active usable => lấy record mới nhất có refresh token để refresh
        var candidate = q.OrderByDescending(x => x.CreationTime)
                         .FirstOrDefault(x => !string.IsNullOrWhiteSpace(x.RefreshToken));

        if (candidate == null)
            throw new BusinessException("ZaloAuth:NotConfigured");

        return candidate;
    }
}