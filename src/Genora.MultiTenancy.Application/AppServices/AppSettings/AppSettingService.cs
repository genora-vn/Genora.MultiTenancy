using Genora.MultiTenancy.AppDtos.AppSettings;
using Genora.MultiTenancy.Apps.AppSettings;
using Genora.MultiTenancy.Features.AppSettings;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Entities.Caching;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.AppSettings;

// Code cũ khi chưa tách FeatureProtectedCrudAppService để dùng chung (cứ để đó)
//[Authorize]
//public class AppSettingService : ApplicationService, IAppSettingService
//{
//    private readonly IRepository<AppSetting, Guid> _repository;
//    private readonly IEntityCache<AppSettingDto, Guid> _appSettingCache;
//    private readonly ICurrentTenant _currentTenant;
//    private readonly IFeatureChecker _featureChecker;

//    public AppSettingService(
//        IRepository<AppSetting, Guid> repository,
//        IEntityCache<AppSettingDto, Guid> appSettingCache,
//        ICurrentTenant currentTenant,
//        IFeatureChecker featureChecker)
//    {
//        _repository = repository;
//        _appSettingCache = appSettingCache;
//        _currentTenant = currentTenant;
//        _featureChecker = featureChecker;
//    }

//    // map quyền TENANT -> quyền HOST khi đang ở host
//    private string MapPermissionForSide(string tenantPermission)
//        => _currentTenant.IsAvailable
//            ? tenantPermission
//            : tenantPermission switch
//            {
//                var x when x == MultiTenancyPermissions.AppSettings.Create => MultiTenancyPermissions.HostAppSettings.Create,
//                var x when x == MultiTenancyPermissions.AppSettings.Edit => MultiTenancyPermissions.HostAppSettings.Edit,
//                var x when x == MultiTenancyPermissions.AppSettings.Delete => MultiTenancyPermissions.HostAppSettings.Delete,
//                _ => MultiTenancyPermissions.HostAppSettings.Default
//            };

//    private async Task EnsureAccessAsync(string tenantPermissionForAction)
//    {
//        // 1) Check quyền: Host dùng HostAppSettings.*, Tenant dùng AppSettings.*
//        await AuthorizationService.CheckAsync(MapPermissionForSide(tenantPermissionForAction));

//        // 2) Chỉ Tenant mới bị ràng buộc Feature
//        if (_currentTenant.IsAvailable &&
//            !await _featureChecker.IsEnabledAsync(AppSettingFeatures.Management))
//        {
//            throw new AbpAuthorizationException("AppSetting feature is disabled for this tenant.");
//        }
//    }

//    public async Task<AppSettingDto> GetAsync(Guid id)
//    {
//        await EnsureAccessAsync(MultiTenancyPermissions.AppSettings.Default);
//        return await _appSettingCache.GetAsync(id);
//    }

//    public async Task<PagedResultDto<AppSettingDto>> GetListAsync(PagedAndSortedResultRequestDto input)
//    {
//        await EnsureAccessAsync(MultiTenancyPermissions.AppSettings.Default);

//        var q = (await _repository.GetQueryableAsync())
//                .OrderBy(input.Sorting.IsNullOrWhiteSpace() ? "SettingKey" : input.Sorting)
//                .Skip(input.SkipCount)
//                .Take(input.MaxResultCount);

//        var items = await AsyncExecuter.ToListAsync(q);
//        var total = await AsyncExecuter.CountAsync(await _repository.GetQueryableAsync());

//        return new PagedResultDto<AppSettingDto>(total, ObjectMapper.Map<List<AppSetting>, List<AppSettingDto>>(items));
//    }

//    public async Task<AppSettingDto> CreateAsync(CreateUpdateAppSettingDto input)
//    {
//        await EnsureAccessAsync(MultiTenancyPermissions.AppSettings.Create);

//        var appSetting = ObjectMapper.Map<CreateUpdateAppSettingDto, AppSetting>(input);
//        await _repository.InsertAsync(appSetting);
//        return ObjectMapper.Map<AppSetting, AppSettingDto>(appSetting);
//    }

//    public async Task<AppSettingDto> UpdateAsync(Guid id, CreateUpdateAppSettingDto input)
//    {
//        await EnsureAccessAsync(MultiTenancyPermissions.AppSettings.Edit);

//        var appSetting = await _repository.GetAsync(id);
//        ObjectMapper.Map(input, appSetting);
//        await _repository.UpdateAsync(appSetting);
//        return ObjectMapper.Map<AppSetting, AppSettingDto>(appSetting);
//    }

//    public async Task DeleteAsync(Guid id)
//    {
//        await EnsureAccessAsync(MultiTenancyPermissions.AppSettings.Delete);
//        await _repository.DeleteAsync(id);
//    }
//}
/// <summary>
/// AppSetting Service implement base FeatureProtectedCrudAppService
/// </summary>
[Authorize]
public class AppSettingService : FeatureProtectedCrudAppService<AppSetting, AppSettingDto, Guid, PagedAndSortedResultRequestDto, CreateUpdateAppSettingDto>, IAppSettingService
{
    private readonly IEntityCache<AppSettingDto, Guid> _appSettingCache;

    protected override string FeatureName => AppSettingFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppSettings.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppSettings.Default;

    public AppSettingService(
        IRepository<AppSetting, Guid> repository,
        IEntityCache<AppSettingDto, Guid> appSettingCache,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        _appSettingCache = appSettingCache;

        GetPolicyName = MultiTenancyPermissions.AppSettings.Default;
        GetListPolicyName = MultiTenancyPermissions.AppSettings.Default;
        CreatePolicyName = MultiTenancyPermissions.AppSettings.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppSettings.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppSettings.Delete;
    }

    public override async Task<AppSettingDto> GetAsync(Guid id)
    {
        // base class đã check permission & feature
        await CheckGetPolicyAsync();
        return await _appSettingCache.GetAsync(id);
    }

    public override async Task<PagedResultDto<AppSettingDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(AppSetting.SettingKey)
            : input.Sorting;

        var query = queryable
            .OrderBy(sorting)
            .Skip(input.SkipCount)
            .Take(input.MaxResultCount);

        var items = await AsyncExecuter.ToListAsync(query);
        var totalCount = await AsyncExecuter.CountAsync(queryable);

        return new PagedResultDto<AppSettingDto>(
            totalCount,
            ObjectMapper.Map<List<AppSetting>, List<AppSettingDto>>(items)
        );
    }

    public override async Task<AppSettingDto> CreateAsync(CreateUpdateAppSettingDto input)
    {
        await CheckCreatePolicyAsync();

        var entity = ObjectMapper.Map<CreateUpdateAppSettingDto, AppSetting>(input);
        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<AppSetting, AppSettingDto>(entity);
    }

    public override async Task<AppSettingDto> UpdateAsync(Guid id, CreateUpdateAppSettingDto input)
    {
        await CheckUpdatePolicyAsync();

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);
        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<AppSetting, AppSettingDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        await Repository.DeleteAsync(id);
    }
}