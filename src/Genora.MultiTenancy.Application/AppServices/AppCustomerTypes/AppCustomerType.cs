using Genora.MultiTenancy.AppDtos.AppCustomerTypes;
using Genora.MultiTenancy.DomainModels.AppCustomerTypes;
using Genora.MultiTenancy.Features.AppCustomerTypes;
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

namespace Genora.MultiTenancy.AppServices.AppCustomerTypes;

[Authorize]
public class AppCustomerTypeService :
        FeatureProtectedCrudAppService<
            CustomerType,
            AppCustomerTypeDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAppCustomerTypeDto>,
        IAppCustomerTypeService
{
    protected override string FeatureName => AppCustomerTypeFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppCustomerTypes.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppCustomerTypes.Default;

    public AppCustomerTypeService(
        IRepository<CustomerType, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppCustomerTypes.Default;
        GetListPolicyName = MultiTenancyPermissions.AppCustomerTypes.Default;
        CreatePolicyName = MultiTenancyPermissions.AppCustomerTypes.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppCustomerTypes.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppCustomerTypes.Delete;
    }

    public override async Task<PagedResultDto<AppCustomerTypeDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(CustomerType.Code)
            : input.Sorting;

        var query = queryable
            .OrderBy(sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<AppCustomerTypeDto>(
            totalCount,
            ObjectMapper.Map<List<CustomerType>, List<AppCustomerTypeDto>>(items)
        );
    }
}