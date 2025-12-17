using Genora.MultiTenancy.AppDtos.AppZaloAuths;
using Genora.MultiTenancy.AppDtos.ZaloAuths;
using Genora.MultiTenancy.DomainModels.AppZaloAuth;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

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
    protected override string FeatureName => ""; // Chỉ cho host quản lý, tenant ko cần
    protected override string TenantDefaultPermission => MultiTenancyPermissions.HostAppZaloAuths.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppZaloAuths.Default;

    private readonly IConfiguration _configuration;

    public AppZaloAuthAppService(
        IRepository<ZaloAuth, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IConfiguration configuration)
        : base(repository, currentTenant, featureChecker)
    {
        _configuration = configuration;

        GetPolicyName = MultiTenancyPermissions.HostAppZaloAuths.Default;
        GetListPolicyName = MultiTenancyPermissions.HostAppZaloAuths.Default;
        CreatePolicyName = MultiTenancyPermissions.HostAppZaloAuths.Create;
        UpdatePolicyName = MultiTenancyPermissions.HostAppZaloAuths.Edit;
        DeletePolicyName = MultiTenancyPermissions.HostAppZaloAuths.Delete;
    }

    private void EnsureHostOnly()
    {
        if (CurrentTenant.IsAvailable)
        {
            throw new AbpAuthorizationException("Host only.");
        }
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
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? "CreationTime desc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var total = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<AppZaloAuthDto>(
            total,
            ObjectMapper.Map<System.Collections.Generic.List<ZaloAuth>, System.Collections.Generic.List<AppZaloAuthDto>>(items)
        );
    }

    public override async Task<AppZaloAuthDto> CreateAsync(CreateUpdateZaloAuthDto input)
    {
        EnsureHostOnly();
        await CheckCreatePolicyAsync();

        // host-only -> thường null
        input.TenantId = null;

        return await base.CreateAsync(input);
    }

    public override async Task<AppZaloAuthDto> UpdateAsync(Guid id, CreateUpdateZaloAuthDto input)
    {
        EnsureHostOnly();
        await CheckUpdatePolicyAsync();

        input.TenantId = null;
        return await base.UpdateAsync(id, input);
    }
}