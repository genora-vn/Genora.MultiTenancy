using Genora.MultiTenancy.AppDtos.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppGolfCourses;
using Genora.MultiTenancy.DomainModels.AppPromotionTypes;
using Genora.MultiTenancy.Features.AppGolfCourses;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp;
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
    private readonly IRepository<PromotionType, Guid> _promotionTypeRepository;

    public AppGolfCourseService(
        IRepository<GolfCourse, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker,
        IRepository<PromotionType, Guid> promotionTypeRepository)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppGolfCourses.Default;
        GetListPolicyName = MultiTenancyPermissions.AppGolfCourses.Default;
        CreatePolicyName = MultiTenancyPermissions.AppGolfCourses.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppGolfCourses.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppGolfCourses.Delete;
        _promotionTypeRepository = promotionTypeRepository;
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

    public override async Task<AppGolfCourseDto> CreateAsync(CreateUpdateAppGolfCourseDto input)
    {
        input.PromotionTypeIds = await NormalizePromotionTypeIdsAsync(input.PromotionTypeIds);
        return await base.CreateAsync(input);
    }

    public override async Task<AppGolfCourseDto> UpdateAsync(Guid id, CreateUpdateAppGolfCourseDto input)
    {
        input.PromotionTypeIds = await NormalizePromotionTypeIdsAsync(input.PromotionTypeIds);
        return await base.UpdateAsync(id, input);
    }

    private async Task<string?> NormalizePromotionTypeIdsAsync(string? promotionTypeIds)
    {
        if (string.IsNullOrWhiteSpace(promotionTypeIds))
        {
            return null;
        }

        var ids = promotionTypeIds
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(x => Guid.TryParse(x, out var id) ? id : Guid.Empty)
            .Where(x => x != Guid.Empty)
            .Distinct()
            .ToList();

        if (ids.Count == 0)
        {
            return null;
        }

        var query = await _promotionTypeRepository.GetQueryableAsync();
        var validIds = await AsyncExecuter.ToListAsync(
            query.Where(x => ids.Contains(x.Id)).Select(x => x.Id)
        );

        if (validIds.Count != ids.Count)
        {
            throw new UserFriendlyException("Có loại ưu đãi không tồn tại.");
        }

        return string.Join(",", validIds);
    }
    // Note: Create/Update/Delete/Get dùng mặc định của CrudAppService (đã check permission & feature)
}