using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Helpers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Security.Encryption;

namespace Genora.MultiTenancy.AppServices.AppZaloAuths;

[Authorize(MultiTenancyPermissions.HostAppZaloAuths.Default)]
public class AppZaloAuthAppService :
    FeatureProtectedCrudAppService<
        ZaloAuth,
        AppZaloAuthDto,
        Guid,
        GetAppZaloAuthListInput,
        CreateUpdateZaloAuthDto>,
    IAppZaloAuthAppService
{
    protected override string FeatureName => "";
    protected override string TenantDefaultPermission => MultiTenancyPermissions.HostAppZaloAuths.Default;
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

        GetPolicyName = MultiTenancyPermissions.HostAppZaloAuths.Default;
        GetListPolicyName = MultiTenancyPermissions.HostAppZaloAuths.Default;
        CreatePolicyName = MultiTenancyPermissions.HostAppZaloAuths.Create;
        UpdatePolicyName = MultiTenancyPermissions.HostAppZaloAuths.Edit;
        DeletePolicyName = MultiTenancyPermissions.HostAppZaloAuths.Delete;
    }

    private void EnsureHostOnly()
    {
        if (CurrentTenant.IsAvailable)
            throw new AbpAuthorizationException("Host only.");
    }

    private AppZaloAuthDto ToSafeDto(ZaloAuth e)
    {
        var dto = ObjectMapper.Map<ZaloAuth, AppZaloAuthDto>(e);

        dto.HasAccessToken = !string.IsNullOrWhiteSpace(e.AccessToken);
        dto.HasRefreshToken = !string.IsNullOrWhiteSpace(e.RefreshToken);

        // access mask
        try
        {
            var accessPlain = SecurityHelper.DecryptMaybe(e.AccessToken, _encrypt);
            dto.AccessTokenMasked = SecurityHelper.MaskToken(accessPlain);
        }
        catch
        {
            dto.AccessTokenMasked = SecurityHelper.MaskToken(e.AccessToken);
        }

        // refresh mask
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
        EnsureHostOnly();
        await CheckGetListPolicyAsync();

        var query = await Repository.GetQueryableAsync();

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
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        var safeDtos = items.Select(ToSafeDto).ToList();
        return new PagedResultDto<AppZaloAuthDto>(total, safeDtos);
    }

    public override async Task<AppZaloAuthDto> GetAsync(Guid id)
    {
        EnsureHostOnly();
        await CheckGetPolicyAsync();

        var entity = await Repository.GetAsync(id);
        return ToSafeDto(entity);
    }

    public override async Task<AppZaloAuthDto> CreateAsync(CreateUpdateZaloAuthDto input)
    {
        EnsureHostOnly();
        await CheckCreatePolicyAsync();

        input.TenantId = null;

        var entity = new ZaloAuth
        {
            TenantId = null,
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

        await Repository.InsertAsync(entity, autoSave: true);

        return ToSafeDto(entity);
    }

    public override async Task<AppZaloAuthDto> UpdateAsync(Guid id, CreateUpdateZaloAuthDto input)
    {
        EnsureHostOnly();
        await CheckUpdatePolicyAsync();

        input.TenantId = null;

        var entity = await Repository.GetAsync(id);

        entity.AppId = input.AppId;
        entity.OaId = input.OaId;
        entity.CodeChallenge = input.CodeChallenge;
        entity.CodeVerifier = input.CodeVerifier;
        entity.State = input.State;
        entity.AuthorizationCode = input.AuthorizationCode;
        entity.ExpireAuthorizationCodeTime = input.ExpireAuthorizationCodeTime;
        entity.IsActive = input.IsActive;

        // ✅ Token chỉ update khi có nhập (không overwrite nếu trống)
        if (!string.IsNullOrWhiteSpace(input.AccessToken))
            entity.AccessToken = SecurityHelper.EncryptMaybe(input.AccessToken, _encrypt);

        if (!string.IsNullOrWhiteSpace(input.RefreshToken))
            entity.RefreshToken = SecurityHelper.EncryptMaybe(input.RefreshToken, _encrypt);

        if (input.ExpireTokenTime.HasValue)
            entity.ExpireTokenTime = input.ExpireTokenTime;

        await Repository.UpdateAsync(entity, autoSave: true);

        return ToSafeDto(entity);
    }
}
