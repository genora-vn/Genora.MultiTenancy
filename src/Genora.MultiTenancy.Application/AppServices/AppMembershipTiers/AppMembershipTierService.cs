using Genora.MultiTenancy.AppDtos.AppMembershipTiers;
using Genora.MultiTenancy.DomainModels.AppMembershipTiers;
using Genora.MultiTenancy.Features.AppMembershipTiers;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppMembershipTiers;

[Authorize]
public class AppMembershipTierService :
        FeatureProtectedCrudAppService<
            MembershipTier,
            AppMembershipTierDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAppMembershipTierDto>,
        IAppMembershipTierService
{
    protected override string FeatureName => AppMembershipTierFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppMembershipTiers.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppMembershipTiers.Default;

    public AppMembershipTierService(
        IRepository<MembershipTier, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppMembershipTiers.Default;
        GetListPolicyName = MultiTenancyPermissions.AppMembershipTiers.Default;
        CreatePolicyName = MultiTenancyPermissions.AppMembershipTiers.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppMembershipTiers.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppMembershipTiers.Delete;
    }

    public override async Task<PagedResultDto<AppMembershipTierDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(MembershipTier.DisplayOrder) + "," + nameof(MembershipTier.Code)
            : input.Sorting;

        var query = queryable
            .OrderBy(sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<AppMembershipTierDto>(
            totalCount,
            ObjectMapper.Map<List<MembershipTier>, List<AppMembershipTierDto>>(items)
        );
    }
}