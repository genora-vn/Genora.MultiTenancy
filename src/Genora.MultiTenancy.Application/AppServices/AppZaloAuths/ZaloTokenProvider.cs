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
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;
using Volo.Abp.Uow;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class ZaloTokenProvider : IZaloTokenProvider
{
    private static readonly SemaphoreSlim _lock = new(1, 1);

    private readonly IRepository<ZaloAuth, Guid> _authRepo;
    private readonly IConfiguration _cfg;
    private readonly IZaloOAuthClient _oauthClient;
    private readonly IStringEncryptionService _encrypt;
    private readonly ICurrentTenant _currentTenant;

    private readonly IUnitOfWorkManager _uowManager;
    private readonly IAsyncQueryableExecuter _asyncExecuter;

    public ZaloTokenProvider(
        IRepository<ZaloAuth, Guid> authRepo,
        IConfiguration cfg,
        IZaloOAuthClient oauthClient,
        IStringEncryptionService encrypt,
        ICurrentTenant currentTenant,
        IUnitOfWorkManager uowManager,
        IAsyncQueryableExecuter asyncExecuter)
    {
        _authRepo = authRepo;
        _cfg = cfg;
        _oauthClient = oauthClient;
        _encrypt = encrypt;
        _currentTenant = currentTenant;

        _uowManager = uowManager;
        _asyncExecuter = asyncExecuter;
    }

    private Guid? ScopeTenantId => _currentTenant.IsAvailable ? _currentTenant.Id : (Guid?)null;

    public async Task<string> GetAccessTokenAsync()
    {
        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: false);

        var auth = await GetBestAuthForUseOrRefreshAsync();

        var access = SecurityHelper.DecryptMaybe(auth.AccessToken, _encrypt);

        var skewSeconds = _cfg.GetValue<int>("Zalo:TokenRefreshSkewSeconds", 60);
        var shouldRefresh =
            string.IsNullOrWhiteSpace(access) ||
            !auth.ExpireTokenTime.HasValue ||
            auth.ExpireTokenTime.Value <= DateTime.UtcNow.AddSeconds(skewSeconds);

        if (!shouldRefresh && auth.IsActive)
        {
            await uow.CompleteAsync();
            return access!;
        }

        await _lock.WaitAsync();
        try
        {
            auth = await GetBestAuthForUseOrRefreshAsync();
            access = SecurityHelper.DecryptMaybe(auth.AccessToken, _encrypt);

            shouldRefresh =
                string.IsNullOrWhiteSpace(access) ||
                !auth.ExpireTokenTime.HasValue ||
                auth.ExpireTokenTime.Value <= DateTime.UtcNow.AddSeconds(skewSeconds) ||
                !auth.IsActive;

            if (!shouldRefresh && auth.IsActive)
            {
                await uow.CompleteAsync();
                return access!;
            }

            var newAuth = await RefreshAndRotateAsync(auth);
            await uow.CompleteAsync();

            return SecurityHelper.DecryptMaybe(newAuth.AccessToken, _encrypt)!;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task RefreshNowAsync()
    {
        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: false);

        var auth = await GetBestAuthForUseOrRefreshAsync();

        await _lock.WaitAsync();
        try
        {
            await RefreshAndRotateAsync(auth);
            await uow.CompleteAsync();
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
            TenantId = current.TenantId,

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
        using var uow = _uowManager.Begin(requiresNew: true, isTransactional: false);

        var q = await _authRepo.GetQueryableAsync();
        q = q.Where(x => x.TenantId == ScopeTenantId);

        var actives = q.Where(x => x.IsActive && x.Id != keepId);

        var list = await _asyncExecuter.ToListAsync(actives);
        if (list.Count == 0)
        {
            await uow.CompleteAsync();
            return;
        }

        foreach (var a in list) a.IsActive = false;
        foreach (var a in list) await _authRepo.UpdateAsync(a, autoSave: true);

        await uow.CompleteAsync();
    }

    private async Task CleanupInactiveHistoryAsync()
    {
        var maxInactive = _cfg.GetValue<int>("Zalo:TokenHistoryMaxInactive", 50);
        if (maxInactive <= 0) return;

        var q = await _authRepo.GetQueryableAsync();
        q = q.Where(x => x.TenantId == ScopeTenantId);

        var oldIdsQuery = q.Where(x => !x.IsActive)
                           .OrderByDescending(x => x.CreationTime)
                           .Skip(maxInactive)
                           .Select(x => x.Id);

        var oldIds = await _asyncExecuter.ToListAsync(oldIdsQuery);

        foreach (var id in oldIds)
        {
            await _authRepo.DeleteAsync(id, autoSave: true);
        }
    }

    private async Task<ZaloAuth> GetBestAuthForUseOrRefreshAsync()
    {
        var q = await _authRepo.GetQueryableAsync();

        // ✅ scope theo tenant hiện tại (tenant hoặc host)
        q = q.Where(x => x.TenantId == ScopeTenantId);

        // ✅ Active mới nhất
        var activeQuery = q.Where(x => x.IsActive)
                           .OrderByDescending(x => x.CreationTime);

        var active = await _asyncExecuter.FirstOrDefaultAsync(activeQuery);

        if (active != null)
        {
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

        // ✅ candidate có refresh token (mới nhất)
        var candidateQuery = q.Where(x => !string.IsNullOrWhiteSpace(x.RefreshToken))
                              .OrderByDescending(x => x.CreationTime);

        var candidate = await _asyncExecuter.FirstOrDefaultAsync(candidateQuery);

        if (candidate == null)
            throw new BusinessException("ZaloAuth:NotConfigured");

        return candidate;
    }
}