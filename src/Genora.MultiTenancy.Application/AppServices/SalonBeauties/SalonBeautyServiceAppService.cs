using System;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServices;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Enums;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauty;

[Authorize]
public class SalonBeautyServiceAppService : ApplicationService, ISalonBeautyServiceAppService
{
    private readonly IRepository<SalonBeautyService, Guid> _repository;
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _categoryRepository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public SalonBeautyServiceAppService(
        IRepository<SalonBeautyService, Guid> repository,
        IRepository<SalonBeautyServiceCategory, Guid> categoryRepository,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _repository = repository;
        _categoryRepository = categoryRepository;
        _l = l;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<PagedResultDto<SalonBeautyServiceDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckServicePolicyAsync(
            MultiTenancyPermissions.SalonBeautyServices.Default,
            MultiTenancyPermissions.HostSalonBeautyServices.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var query = await _repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText!.Trim();
            query = query.Where(x =>
                x.Name.Contains(keyword) ||
                x.Note != null && x.Note.Contains(keyword));
        }

        if (input.CategoryId.HasValue && input.CategoryId.Value != Guid.Empty)
            query = query.Where(x => x.CategoryId == input.CategoryId.Value);

        if (input.ApplicableRole.HasValue)
            query = query.Where(x => x.ApplicableRole == input.ApplicableRole.Value);
        else if (input.Role.HasValue)
            query = query.Where(x => x.ApplicableRole == input.Role.Value);

        if (input.ApplicableLevel.HasValue)
            query = query.Where(x => x.ApplicableLevel == input.ApplicableLevel.Value);
        else if (input.Level.HasValue)
            query = query.Where(x => x.ApplicableLevel == input.Level.Value);

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        if (input.IsShowOnApp.HasValue)
            query = query.Where(x => x.IsShowOnApp == input.IsShowOnApp.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query
                .OrderBy(x => x.SortOrder)
                .ThenBy(x => x.Name)
                .Skip(input.SkipCount)
                .Take(input.MaxResultCount));

        var categoryIds = items.Select(x => x.CategoryId).Distinct().ToList();
        var categories = categoryIds.Count == 0
            ? new System.Collections.Generic.List<SalonBeautyServiceCategory>()
            : await _categoryRepository.GetListAsync(x => categoryIds.Contains(x.Id));
        var categoryDict = categories.ToDictionary(x => x.Id, x => x.Name);

        return new PagedResultDto<SalonBeautyServiceDto>(
            totalCount,
            items.Select(x => MapToDto(x, categoryDict.TryGetValue(x.CategoryId, out var catName) ? catName : null)).ToList());
    }

    public async Task<SalonBeautyServiceDto> GetAsync(Guid id)
    {
        await CheckServicePolicyAsync(
            MultiTenancyPermissions.SalonBeautyServices.Default,
            MultiTenancyPermissions.HostSalonBeautyServices.Default);

        var entity = await _repository.GetAsync(id);
        var category = await _categoryRepository.FindAsync(entity.CategoryId);
        return MapToDto(entity, category?.Name);
    }

    public async Task<SalonBeautyServiceDto> CreateAsync(CreateSalonBeautyServiceDto input)
    {
        await CheckServicePolicyAsync(
            MultiTenancyPermissions.SalonBeautyServices.Create,
            MultiTenancyPermissions.HostSalonBeautyServices.Create);

        await NormalizeAndValidateAsync(
            input.Name,
            input.CategoryId,
            input.Price,
            input.Duration,
            input.ApplicableRole,
            input.ApplicableLevel,
            input.Status,
            input.IsShowOnApp,
            input.SortOrder,
            null);

        var entity = new SalonBeautyService
        {
            Name = input.Name.Trim(),
            CategoryId = input.CategoryId,
            Price = input.Price,
            Duration = input.Duration,
            ApplicableRole = input.ApplicableRole,
            ApplicableLevel = input.ApplicableLevel,
            Status = input.Status,
            IsShowOnApp = input.IsShowOnApp,
            Note = NullIfWhiteSpace(input.Note),
            SortOrder = input.SortOrder
        };

        var created = await _repository.InsertAsync(entity, autoSave: true);
        var category = await _categoryRepository.FindAsync(created.CategoryId);
        return MapToDto(created, category?.Name);
    }

    public async Task<SalonBeautyServiceDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceDto input)
    {
        await CheckServicePolicyAsync(
            MultiTenancyPermissions.SalonBeautyServices.Edit,
            MultiTenancyPermissions.HostSalonBeautyServices.Edit);

        var entity = await _repository.GetAsync(id);

        await NormalizeAndValidateAsync(
            input.Name,
            input.CategoryId,
            input.Price,
            input.Duration,
            input.ApplicableRole,
            input.ApplicableLevel,
            input.Status,
            input.IsShowOnApp,
            input.SortOrder,
            id);

        entity.Name = input.Name.Trim();
        entity.CategoryId = input.CategoryId;
        entity.Price = input.Price;
        entity.Duration = input.Duration;
        entity.ApplicableRole = input.ApplicableRole;
        entity.ApplicableLevel = input.ApplicableLevel;
        entity.Status = input.Status;
        entity.IsShowOnApp = input.IsShowOnApp;
        entity.Note = NullIfWhiteSpace(input.Note);
        entity.SortOrder = input.SortOrder;

        var updated = await _repository.UpdateAsync(entity, autoSave: true);
        var category = await _categoryRepository.FindAsync(updated.CategoryId);
        return MapToDto(updated, category?.Name);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckServicePolicyAsync(
            MultiTenancyPermissions.SalonBeautyServices.Delete,
            MultiTenancyPermissions.HostSalonBeautyServices.Delete);

        await _repository.DeleteAsync(id, autoSave: true);
    }

    private async Task NormalizeAndValidateAsync(
        string? name,
        Guid categoryId,
        decimal price,
        int duration,
        byte? applicableRole,
        byte? applicableLevel,
        byte status,
        bool isShowOnApp,
        int sortOrder,
        Guid? currentId)
    {
        if (name.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("SalonBeautyServices:NameRequired"));

        if (name!.Trim().Length > 255)
            throw new UserFriendlyException(L("SalonBeautyServices:NameMaxLength"));

        if (categoryId == Guid.Empty || !await _categoryRepository.AnyAsync(x => x.Id == categoryId))
            throw new UserFriendlyException(L("SalonBeautyServices:CategoryRequired"));

        if (price < 0)
            throw new UserFriendlyException(L("SalonBeautyServices:PriceInvalid"));

        if (duration <= 0)
            throw new UserFriendlyException(L("SalonBeautyServices:DurationInvalid"));

        if (!applicableRole.HasValue || !Enum.IsDefined(typeof(SalonBeautyStylistRole), applicableRole.Value))
            throw new UserFriendlyException(L("SalonBeautyServices:RoleRequired"));

        if (!applicableLevel.HasValue || !Enum.IsDefined(typeof(SalonBeautyStylistLevel), applicableLevel.Value))
            throw new UserFriendlyException(L("SalonBeautyServices:LevelRequired"));

        if (status != 0 && status != 1)
            throw new UserFriendlyException(L("SalonBeautyServices:StatusInvalid"));

        if (isShowOnApp && status != 1)
            throw new UserFriendlyException(L("SalonBeautyServices:ShowOnAppRequiresActive"));

        if (sortOrder < 0)
            throw new UserFriendlyException(L("SalonBeautyServices:SortOrderInvalid"));

        var normalizedName = name.Trim();
        var duplicate = currentId.HasValue
            ? await _repository.AnyAsync(x => x.Id != currentId.Value && x.Name == normalizedName)
            : await _repository.AnyAsync(x => x.Name == normalizedName);

        if (duplicate)
            throw new UserFriendlyException(L("SalonBeautyServices:DuplicateName"));
    }

    private string L(string key)
    {
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? key : text;
    }

    private string? LocalizeEnum<TEnum>(byte? value) where TEnum : struct, Enum
    {
        if (!value.HasValue || !Enum.IsDefined(typeof(TEnum), value.Value))
            return null;

        var enumValue = (TEnum)Enum.ToObject(typeof(TEnum), value.Value);
        var key = $"Enum:{typeof(TEnum).Name}.{enumValue}";
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? enumValue.ToString() : text;
    }

    private static string? NullIfWhiteSpace(string? value)
        => value.IsNullOrWhiteSpace() ? null : value.Trim();

    private async Task CheckServicePolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty service permission.");

        await AuthorizationService.CheckAsync(permission);
    }

    private SalonBeautyServiceDto MapToDto(SalonBeautyService entity, string? categoryName)
    {
        var active = entity.Status == 1;
        return new SalonBeautyServiceDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            CategoryName = categoryName,
            Price = entity.Price,
            PriceText = string.Format(CultureInfo.CurrentCulture, "{0:N0}", entity.Price),
            Duration = entity.Duration,
            DurationText = string.Format(CultureInfo.CurrentCulture, L("SalonBeautyServices:DurationMinutesFormat"), entity.Duration),
            ApplicableRole = entity.ApplicableRole,
            ApplicableRoleText = LocalizeEnum<SalonBeautyStylistRole>(entity.ApplicableRole),
            ApplicableLevel = entity.ApplicableLevel,
            ApplicableLevelText = LocalizeEnum<SalonBeautyStylistLevel>(entity.ApplicableLevel),
            Status = entity.Status,
            StatusText = active ? L("SalonBeautyCustomer:StatusActive") : L("SalonBeautyCustomer:StatusInactive"),
            IsShowOnApp = entity.IsShowOnApp,
            IsShowOnAppText = entity.IsShowOnApp ? L("Yes") : L("No"),
            Note = entity.Note,
            SortOrder = entity.SortOrder,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }
}

[Authorize]
public class SalonBeautyServiceCategoryAppService : ApplicationService, ISalonBeautyServiceCategoryAppService
{
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _repository;
    private readonly IStringLocalizer<MultiTenancyResource> _l;

    public SalonBeautyServiceCategoryAppService(
        IRepository<SalonBeautyServiceCategory, Guid> repository,
        IStringLocalizer<MultiTenancyResource> l)
    {
        _repository = repository;
        _l = l;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    public async Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckServiceCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 10 : Math.Min(input.MaxResultCount, 100);

        var query = await _repository.GetQueryableAsync();

        if (!input.FilterText.IsNullOrWhiteSpace())
        {
            var keyword = input.FilterText!.Trim();
            query = query.Where(x => x.Name.Contains(keyword) ||
                                     x.Description != null && x.Description.Contains(keyword) ||
                                     x.Note != null && x.Note.Contains(keyword));
        }

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name)
                 .Skip(input.SkipCount)
                 .Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyServiceCategoryDto>(totalCount, items.Select(MapToDto).ToList());
    }

    public async Task<SalonBeautyServiceCategoryDto> GetAsync(Guid id)
    {
        await CheckServiceCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default);
        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<SalonBeautyServiceCategoryDto> CreateAsync(CreateSalonBeautyServiceCategoryDto input)
    {
        await CheckServiceCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Create);
        await ValidateAsync(input.Name, input.SortOrder, input.Status, null);

        var entity = new SalonBeautyServiceCategory
        {
            Name = input.Name.Trim(),
            Description = NullIfWhiteSpace(input.Description),
            SortOrder = input.SortOrder,
            Status = input.Status,
            Note = NullIfWhiteSpace(input.Note)
        };

        var created = await _repository.InsertAsync(entity, autoSave: true);
        return MapToDto(created);
    }

    public async Task<SalonBeautyServiceCategoryDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceCategoryDto input)
    {
        await CheckServiceCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Edit);
        await ValidateAsync(input.Name, input.SortOrder, input.Status, id);

        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name.Trim();
        entity.Description = NullIfWhiteSpace(input.Description);
        entity.SortOrder = input.SortOrder;
        entity.Status = input.Status;
        entity.Note = NullIfWhiteSpace(input.Note);

        var updated = await _repository.UpdateAsync(entity, autoSave: true);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckServiceCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Delete);
        await _repository.DeleteAsync(id, autoSave: true);
    }

    private async Task ValidateAsync(string? name, int sortOrder, byte status, Guid? currentId)
    {
        if (name.IsNullOrWhiteSpace())
            throw new UserFriendlyException(L("SalonBeautyServiceCategories:NameRequired"));

        if (name!.Trim().Length > 255)
            throw new UserFriendlyException(L("SalonBeautyServiceCategories:NameMaxLength"));

        if (sortOrder < 0)
            throw new UserFriendlyException(L("SalonBeautyServiceCategories:SortOrderInvalid"));

        if (status != 0 && status != 1)
            throw new UserFriendlyException(L("SalonBeautyServiceCategories:StatusInvalid"));

        var normalizedName = name.Trim();
        var duplicate = currentId.HasValue
            ? await _repository.AnyAsync(x => x.Id != currentId.Value && x.Name == normalizedName)
            : await _repository.AnyAsync(x => x.Name == normalizedName);

        if (duplicate)
            throw new UserFriendlyException(L("SalonBeautyServiceCategories:DuplicateName"));
    }

    private SalonBeautyServiceCategoryDto MapToDto(SalonBeautyServiceCategory entity)
    {
        var active = entity.Status == 1;
        return new SalonBeautyServiceCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            Status = entity.Status,
            StatusText = active ? L("SalonBeautyCustomer:StatusActive") : L("SalonBeautyCustomer:StatusInactive"),
            Note = entity.Note,
            CreationTime = entity.CreationTime,
            LastModificationTime = entity.LastModificationTime
        };
    }

    private string L(string key)
    {
        var text = _l[key].Value;
        return text.IsNullOrWhiteSpace() || text == key ? key : text;
    }

    private static string? NullIfWhiteSpace(string? value)
        => value.IsNullOrWhiteSpace() ? null : value.Trim();

    private async Task CheckServiceCategoryPolicyAsync(string tenantPermission)
    {
        if (tenantPermission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty service category permission.");

        var permission = CurrentTenant.IsAvailable
            ? tenantPermission
            : MapSalonBeautyHostPermission(tenantPermission);

        await AuthorizationService.CheckAsync(permission);
    }

    private static string MapSalonBeautyHostPermission(string tenantPermission)
    {
        if (tenantPermission.StartsWith("MultiTenancy.SalonBeautyServiceCategories", StringComparison.Ordinal))
            return tenantPermission.Replace("MultiTenancy.SalonBeautyServiceCategories", "MultiTenancy.HostSalonBeautyServiceCategories");

        if (tenantPermission.Contains(".SalonBeautyServiceCategories", StringComparison.Ordinal))
            return tenantPermission.Replace(".SalonBeautyServiceCategories", ".HostSalonBeautyServiceCategories");

        return tenantPermission.StartsWith("SalonBeautyServiceCategories", StringComparison.Ordinal)
            ? "Host" + tenantPermission
            : tenantPermission;
    }
}
