using Genora.MultiTenancy.AppDtos.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbCategories;
using Genora.MultiTenancy.DomainModels.AppFnbItems;
using Genora.MultiTenancy.DomainModels.AppFnbOrders;
using Genora.MultiTenancy.Features.AppFnbFeatures;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Validation;

namespace Genora.MultiTenancy.AppServices.AppFnbItems;

[Authorize]
public class AppFnbItemService :
    FeatureProtectedCrudAppService<FnbItem, FnbItemDto, Guid, GetFnbItemListInput, CreateUpdateFnbItemDto>,
    IAppFnbItemService
{
    protected override string FeatureName => AppFnbFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppFnbItems.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppFnbItems.Default;

    private readonly IRepository<FnbCategory, Guid> _categoryRepository;
    private readonly IRepository<FnbOrderItem, Guid> _orderItemRepository;

    public AppFnbItemService(
        IRepository<FnbItem, Guid> repository,
        IRepository<FnbCategory, Guid> categoryRepository,
        IRepository<FnbOrderItem, Guid> orderItemRepository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        GetPolicyName = MultiTenancyPermissions.AppFnbItems.Default;
        GetListPolicyName = MultiTenancyPermissions.AppFnbItems.Default;
        CreatePolicyName = MultiTenancyPermissions.AppFnbItems.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppFnbItems.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppFnbItems.Delete;

        _categoryRepository = categoryRepository;
        _orderItemRepository = orderItemRepository;
    }

    [DisableValidation]
    public override async Task<PagedResultDto<FnbItemDto>> GetListAsync(GetFnbItemListInput input)
    {
        await CheckGetListPolicyAsync();

        var itemQuery = await Repository.GetQueryableAsync();
        var categoryQuery = await _categoryRepository.GetQueryableAsync();

        var query =
            from item in itemQuery
            join category in categoryQuery on item.CategoryId equals category.Id
            select new { item, category };

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var filter = input.FilterText.Trim();
            query = query.Where(x => x.item.Name.Contains(filter) || x.category.Name.Contains(filter));
        }

        if (input.CategoryId.HasValue)
        {
            query = query.Where(x => x.item.CategoryId == input.CategoryId.Value);
        }

        if (input.IsActive.HasValue)
        {
            query = query.Where(x => x.item.IsActive == input.IsActive.Value);
        }

        if (input.IsAvailable.HasValue)
        {
            query = query.Where(x => x.item.IsAvailable == input.IsAvailable.Value);
        }

        var totalCount = await AsyncExecuter.CountAsync(query);

        var rows = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.category.SortOrder)
                 .ThenBy(x => x.item.SortOrder)
                 .ThenBy(x => x.item.Name)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount)
        );

        var result = rows.Select(x => new FnbItemDto
        {
            Id = x.item.Id,
            TenantId = x.item.TenantId,
            CategoryId = x.item.CategoryId,
            CategoryName = x.category.Name,
            Name = x.item.Name,
            Price = x.item.Price,
            ImageUrl = x.item.ImageUrl,
            Description = x.item.Description,
            IsActive = x.item.IsActive,
            IsAvailable = x.item.IsAvailable,
            SortOrder = x.item.SortOrder,
            CreationTime = x.item.CreationTime,
            CreatorId = x.item.CreatorId,
            LastModificationTime = x.item.LastModificationTime,
            LastModifierId = x.item.LastModifierId,
            IsDeleted = x.item.IsDeleted,
            DeleterId = x.item.DeleterId,
            DeletionTime = x.item.DeletionTime
        }).ToList();

        return new PagedResultDto<FnbItemDto>(totalCount, result);
    }

    public override async Task<FnbItemDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();

        var entity = await Repository.GetAsync(id);
        var category = await _categoryRepository.GetAsync(entity.CategoryId);

        return new FnbItemDto
        {
            Id = entity.Id,
            TenantId = entity.TenantId,
            CategoryId = entity.CategoryId,
            CategoryName = category.Name,
            Name = entity.Name,
            Price = entity.Price,
            ImageUrl = entity.ImageUrl,
            Description = entity.Description,
            IsActive = entity.IsActive,
            IsAvailable = entity.IsAvailable,
            SortOrder = entity.SortOrder,
            CreationTime = entity.CreationTime,
            CreatorId = entity.CreatorId,
            LastModificationTime = entity.LastModificationTime,
            LastModifierId = entity.LastModifierId,
            IsDeleted = entity.IsDeleted,
            DeleterId = entity.DeleterId,
            DeletionTime = entity.DeletionTime
        };
    }

    public override async Task<FnbItemDto> CreateAsync(CreateUpdateFnbItemDto input)
    {
        await CheckCreatePolicyAsync();

        await ValidateCreateUpdateAsync(input);

        var entity = ObjectMapper.Map<CreateUpdateFnbItemDto, FnbItem>(input);
        entity.TenantId = CurrentTenant.Id;
        entity.SortOrder = input.SortOrder ?? await GetNextSortOrderAsync(input.CategoryId);
        entity.IsActive = input.IsActive;
        entity.IsAvailable = input.IsAvailable;

        entity = await Repository.InsertAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public override async Task<FnbItemDto> UpdateAsync(Guid id, CreateUpdateFnbItemDto input)
    {
        await CheckUpdatePolicyAsync();

        await ValidateCreateUpdateAsync(input);

        var entity = await Repository.GetAsync(id);
        ObjectMapper.Map(input, entity);

        if (input.SortOrder.HasValue)
            entity.SortOrder = input.SortOrder.Value;

        entity = await Repository.UpdateAsync(entity, autoSave: true);
        return await GetAsync(entity.Id);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();

        var entity = await Repository.GetAsync(id);

        var hasOrder = await _orderItemRepository.AnyAsync(x => x.ItemId == id);
        if (hasOrder)
        {
            throw new UserFriendlyException("Không thể xóa món vì đã phát sinh đơn hàng. Hãy chuyển sang ngừng hiển thị.");
        }

        await Repository.HardDeleteAsync(entity, autoSave: true);
    }

    private async Task ValidateCreateUpdateAsync(CreateUpdateFnbItemDto input)
    {
        var category = await _categoryRepository.FirstOrDefaultAsync(x => x.Id == input.CategoryId);
        if (category == null)
        {
            throw new UserFriendlyException("Danh mục không tồn tại.");
        }

        if (input.Price < 0)
        {
            throw new UserFriendlyException("Giá món phải lớn hơn hoặc bằng 0.");
        }

        if (input.SortOrder.HasValue && input.SortOrder.Value < 0)
        {
            throw new AbpValidationException("Validation failed");
        }
    }

    private async Task<int> GetNextSortOrderAsync(Guid categoryId)
    {
        var queryable = await Repository.GetQueryableAsync();
        var max = await AsyncExecuter.MaxAsync(
            queryable.Where(x => x.CategoryId == categoryId).Select(x => (int?)x.SortOrder)
        );
        return (max ?? -1) + 1;
    }
}
