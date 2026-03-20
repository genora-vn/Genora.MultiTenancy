using Genora.MultiTenancy.AppDtos.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.Features.AppFnbFeatures;
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
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppFnbCategories;

[Authorize]
public class AppFnbCategoryService :
    FeatureProtectedCrudAppService<FnbCategory, FnbCategoryDto, Guid, GetFnbCategoryListInput, CreateUpdateFnbCategoryDto>,
    IAppFnbCategoryService
{
    protected override string FeatureName => AppFnbFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppFnbCategories.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppFnbCategories.Default;

    private readonly IRepository<FnbItem, Guid> _itemRepository;

    public AppFnbCategoryService(
        IRepository<FnbCategory, Guid> repository,
        IRepository<FnbItem, Guid> itemRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppFnbCategories.Default;
        GetListPolicyName = MultiTenancyPermissions.AppFnbCategories.Default;
        CreatePolicyName = MultiTenancyPermissions.AppFnbCategories.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppFnbCategories.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppFnbCategories.Delete;

        _itemRepository = itemRepository;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<FnbCategoryDto>> GetListAsync(GetFnbCategoryListInput input)
    {
        await CheckGetListPolicyAsync();

        var queryable = await Repository.GetQueryableAsync();
        var query = queryable;

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.Name.Contains(filter) || (x.Code != null && x.Code.Contains(filter)));
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.IsActive == input.IsActive.Value);
        }

        var sorting = string.IsNullOrWhiteSpace(input.Sorting)
            ? nameof(FnbCategory.SortOrder) + " asc, " + nameof(FnbCategory.Name) + " asc"
            : input.Sorting;

        query = query.OrderBy(sorting);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.Skip(input.SkipCount).Take(input.MaxResultCount));
        var dtoList = ObjectMapper.Map<List<FnbCategory>, List<FnbCategoryDto>>(items);

        return new PagedResultDto<FnbCategoryDto>(totalCount, dtoList);
    }

    public override async Task<FnbCategoryDto> CreateAsync(CreateUpdateFnbCategoryDto input)
    {
        await CheckCreatePolicyAsync();

        await ValidateCreateUpdateAsync(input);

        var entity = ObjectMapper.Map<CreateUpdateFnbCategoryDto, FnbCategory>(input);
        entity.TenantId = CurrentTenant.Id;
        entity.SortOrder = input.SortOrder ?? await GetNextSortOrderAsync();
        entity.IsActive = input.IsActive;

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return ObjectMapper.Map<FnbCategory, FnbCategoryDto>(entity);
    }

    public override async Task<FnbCategoryDto> UpdateAsync(Guid id, CreateUpdateFnbCategoryDto input)
    {
        await CheckUpdatePolicyAsync();

        await ValidateCreateUpdateAsync(input, id);

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);

        if (input.SortOrder.HasValue)
            entity.SortOrder = input.SortOrder.Value;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return ObjectMapper.Map<FnbCategory, FnbCategoryDto>(entity);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var entity = await Repository.GetAsync(id);

        var hasItems = await _itemRepository.AnyAsync(x => x.CategoryId == id);
        if (hasItems)
        {
            throw new UserFriendlyException("Không thể xóa danh mục vì vẫn còn món thuộc danh mục này.");
        }

        await Repository.HardDeleteAsync(entity, autoSave: true);
    }

    private async Task ValidateCreateUpdateAsync(CreateUpdateFnbCategoryDto input, Guid? editingId = null)
    {
        if (input.SortOrder.HasValue && input.SortOrder.Value < 0)
        {
            throw new AbpValidationException("Validation failed");
        }

        if (!string.IsNullOrWhiteSpace(input.Code))
        {
            var code = input.Code.Trim();

            var existing = await Repository.FirstOrDefaultAsync(x =>
                x.TenantId == CurrentTenant.Id &&
                x.Code == code &&
                (!editingId.HasValue || x.Id != editingId.Value));

            if (existing != null)
            {
                throw new UserFriendlyException("Mã danh mục đã tồn tại.");
            }
        }
    }

    private async Task<int> GetNextSortOrderAsync()
    {
        var queryable = await Repository.GetQueryableAsync();
        var max = await AsyncExecuter.MaxAsync(queryable.Select(x => (int?)x.SortOrder));
        return (max ?? -1) + 1;
    }
}
