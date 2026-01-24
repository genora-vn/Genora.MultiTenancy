using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Features.AppZaloAuths;
using Genora.MultiTenancy.Permissions;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

public class AppZaloAuthAppService :
    FeatureProtectedCrudAppService<
        ZaloAuth,
        AppZaloAuthDto,
        Guid,
        GetAppZaloAuthListInput,
        CreateUpdateZaloAuthDto>,
    IAppZaloAuthAppService
{
    protected override string FeatureName => AppZaloAuthFeatures.Management;

    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppZaloAuths.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppZaloAuths.Default;

    private readonly IStringEncryptionService _encrypt;

    public AppZaloAuthAppService(
        IRepository<ZaloAuth, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IConfiguration configuration,
        IStringEncryptionService encrypt)
        : base(repository, currentTenant, featureChecker)
    {
        _encrypt = encrypt;

        // ✅ QUAN TRỌNG: không set cứng tenant permission ở đây nữa
        // base class sẽ tự chọn theo side (Host/Tenant) từ TenantDefaultPermission/HostDefaultPermission
        GetPolicyName = MultiTenancyPermissions.AppZaloAuths.Default;
        GetListPolicyName = MultiTenancyPermissions.AppZaloAuths.Default;
        CreatePolicyName = MultiTenancyPermissions.AppZaloAuths.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppZaloAuths.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppZaloAuths.Delete;
    }

    private Guid? GetScopeTenantId()
        => CurrentTenant.IsAvailable ? CurrentTenant.Id : null;

    private AppZaloAuthDto ToSafeDto(ZaloAuth e)
    {
        var dto = ObjectMapper.Map<ZaloAuth, AppZaloAuthDto>(e);

        dto.HasAccessToken = !string.IsNullOrWhiteSpace(e.AccessToken);
        dto.HasRefreshToken = !string.IsNullOrWhiteSpace(e.RefreshToken);

        try
        {
            var accessPlain = SecurityHelper.DecryptMaybe(e.AccessToken, _encrypt);
            dto.AccessTokenMasked = SecurityHelper.MaskToken(accessPlain);
        }
        catch
        {
            dto.AccessTokenMasked = SecurityHelper.MaskToken(e.AccessToken);
        }

        try
        {
            var refreshPlain = SecurityHelper.DecryptMaybe(e.RefreshToken, _encrypt);
            dto.RefreshTokenMasked = SecurityHelper.MaskToken(refreshPlain);
        }
        catch
        {
            dto.RefreshTokenMasked = SecurityHelper.MaskToken(e.RefreshToken);
        }

        return dto;
    }

    public override async Task<PagedResultDto<AppZaloAuthDto>> GetListAsync(GetAppZaloAuthListInput input)
    {
        await CheckGetListPolicyAsync();

        var scopeTenantId = GetScopeTenantId();

        using (CurrentTenant.Change(scopeTenantId))
        {
            var query = await Repository.GetQueryableAsync();

            query = query.Where(x => x.TenantId == scopeTenantId);

            if (!string.IsNullOrWhiteSpace(input.FilterText))
            {
                var ft = input.FilterText.Trim();
                query = query.Where(x =>
                    x.AppId.Contains(ft) ||
                    (x.State != null && x.State.Contains(ft)) ||
                    (x.AuthorizationCode != null && x.AuthorizationCode.Contains(ft))
                );
            }

            if (input.IsActive.HasValue)
                query = query.Where(x => x.IsActive == input.IsActive.Value);

            var sorting = string.IsNullOrWhiteSpace(input.Sorting)
                ? "CreationTime desc"
                : input.Sorting;

            query = query.OrderBy(sorting);

            var total = await AsyncExecuter.CountAsync(query);
            var items = await AsyncExecuter.ToListAsync(
                query.Skip(input.SkipCount).Take(input.MaxResultCount)
            );

            var safeDtos = items.Select(ToSafeDto).ToList();
            return new PagedResultDto<AppZaloAuthDto>(total, safeDtos);
        }
    }

    public override async Task<AppZaloAuthDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var scopeTenantId = GetScopeTenantId();
        using (CurrentTenant.Change(scopeTenantId))
        {
            var entity = await Repository.GetAsync(id);

            if (entity.TenantId != scopeTenantId)
                throw new AbpAuthorizationException("Not allowed.");

            return ToSafeDto(entity);
        }
    }

    public override async Task<AppZaloAuthDto> CreateAsync(CreateUpdateZaloAuthDto input)
    {
        await CheckCreatePolicyAsync();

        var scopeTenantId = GetScopeTenantId();
        using (CurrentTenant.Change(scopeTenantId))
        {
            input.TenantId = scopeTenantId;

            var entity = new ZaloAuth
            {
                TenantId = scopeTenantId,
                AppId = input.AppId,
                OaId = input.OaId,
                CodeChallenge = input.CodeChallenge,
                CodeVerifier = input.CodeVerifier,
                State = input.State,
                AuthorizationCode = input.AuthorizationCode,
                ExpireAuthorizationCodeTime = input.ExpireAuthorizationCodeTime,
                ExpireTokenTime = input.ExpireTokenTime,
                IsActive = input.IsActive
            };

            if (!string.IsNullOrWhiteSpace(input.AccessToken))
                entity.AccessToken = SecurityHelper.EncryptMaybe(input.AccessToken, _encrypt);

            if (!string.IsNullOrWhiteSpace(input.RefreshToken))
                entity.RefreshToken = SecurityHelper.EncryptMaybe(input.RefreshToken, _encrypt);

            if (entity.IsActive && entity.ExpireTokenTime.HasValue && entity.ExpireTokenTime.Value <= DateTime.UtcNow)
                entity.IsActive = false;

            await Repository.InsertAsync(entity, autoSave: true);

            if (entity.IsActive)
                await ZaloAuthActiveNormalizer.SetActiveOnlyAsync(Repository, entity.Id);

            return ToSafeDto(entity);
        }
    }

    public override async Task<AppZaloAuthDto> UpdateAsync(Guid id, CreateUpdateZaloAuthDto input)
    {
        await CheckUpdatePolicyAsync();

        var scopeTenantId = GetScopeTenantId();
        using (CurrentTenant.Change(scopeTenantId))
        {
            input.TenantId = scopeTenantId;

            var entity = await Repository.GetAsync(id);

            if (entity.TenantId != scopeTenantId)
                throw new AbpAuthorizationException("Not allowed.");

            entity.AppId = input.AppId;
            entity.OaId = input.OaId;
            entity.CodeChallenge = input.CodeChallenge;
            entity.CodeVerifier = input.CodeVerifier;
            entity.State = input.State;
            entity.AuthorizationCode = input.AuthorizationCode;
            entity.ExpireAuthorizationCodeTime = input.ExpireAuthorizationCodeTime;
            entity.IsActive = input.IsActive;

            if (!string.IsNullOrWhiteSpace(input.AccessToken))
                entity.AccessToken = SecurityHelper.EncryptMaybe(input.AccessToken, _encrypt);

            if (!string.IsNullOrWhiteSpace(input.RefreshToken))
                entity.RefreshToken = SecurityHelper.EncryptMaybe(input.RefreshToken, _encrypt);

            if (input.ExpireTokenTime.HasValue)
                entity.ExpireTokenTime = input.ExpireTokenTime;

            if (entity.IsActive && entity.ExpireTokenTime.HasValue && entity.ExpireTokenTime.Value <= DateTime.UtcNow)
                entity.IsActive = false;

            await Repository.UpdateAsync(entity, autoSave: true);

            if (entity.IsActive)
                await ZaloAuthActiveNormalizer.SetActiveOnlyAsync(Repository, entity.Id);

            return ToSafeDto(entity);
        }
    }
}
