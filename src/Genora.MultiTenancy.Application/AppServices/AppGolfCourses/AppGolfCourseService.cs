using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.Features.AppGolfCourses;
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

namespace Genora.MultiTenancy.AppServices.AppGolfCourses;

[Authorize]
public class AppGolfCourseService :
        FeatureProtectedCrudAppService<
            GolfCourse,
            AppGolfCourseDto,
            Guid,
            PagedAndSortedResultRequestDto,
            CreateUpdateAppGolfCourseDto>,
        IAppGolfCourseService
{
    protected override string FeatureName => AppGolfCourseFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppGolfCourses.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppGolfCourses.Default;

    public AppGolfCourseService(
        IRepository<GolfCourse, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppGolfCourses.Default;
        GetListPolicyName = MultiTenancyPermissions.AppGolfCourses.Default;
        CreatePolicyName = MultiTenancyPermissions.AppGolfCourses.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppGolfCourses.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppGolfCourses.Delete;
    }

    public override async Task<PagedResultDto<AppGolfCourseDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(GolfCourse.Code)
            : input.Sorting;

        var query = queryable
            .OrderBy(sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<AppGolfCourseDto>(
            totalCount,
            ObjectMapper.Map<List<GolfCourse>, List<AppGolfCourseDto>>(items)
        );
    }

    // Note: Create/Update/Delete/Get dùng mặc định của CrudAppService (đã check permission & feature)
}