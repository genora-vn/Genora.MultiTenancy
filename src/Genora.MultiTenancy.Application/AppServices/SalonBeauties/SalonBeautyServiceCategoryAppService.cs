using System;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeauties;
using Genora.MultiTenancy.AppDtos.SalonBeauties.SalonBeautyServiceCategories;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.AppServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Localization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.Linq;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.SalonBeauties;

[Authorize]
public class SalonBeautyServiceCategoryAppService :
    FeatureProtectedCrudAppService<
        SalonBeautyServiceCategory,
        SalonBeautyServiceCategoryDto,
        Guid,
        GetSalonBeautyListInput,
        CreateSalonBeautyServiceCategoryDto,
        UpdateSalonBeautyServiceCategoryDto>,
    ISalonBeautyServiceCategoryAppService
{
    protected override string FeatureName => string.Empty;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.SalonBeautyServiceCategories.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostSalonBeautyServiceCategories.Default;

    private readonly IRepository<SalonBeautyServiceCategory, Guid> _repository;

    public SalonBeautyServiceCategoryAppService(
        IRepository<SalonBeautyServiceCategory, Guid> repository,
        ICurrentTenant currentTenant,
        IFeatureChecker featureChecker)
        : base(repository, currentTenant, featureChecker)
    {
        _repository = repository;
    }

    public override async Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default, MultiTenancyPermissions.HostSalonBeautyServiceCategories.Default);

        input.MaxResultCount = input.MaxResultCount <= 0 ? 100 : Math.Min(input.MaxResultCount, 1000);

        var query = await _repository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(), x => x.Name.Contains(input.FilterText!));

        if (input.Status.HasValue)
            query = query.Where(x => x.Status == input.Status.Value);

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder).ThenBy(x => x.Name).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyServiceCategoryDto>
        {
            TotalCount = totalCount,
            Items = items.Select(MapToDto).ToList()
        };
    }

    public override async Task<SalonBeautyServiceCategoryDto> GetAsync(Guid id)
    {
        await CheckCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default, MultiTenancyPermissions.HostSalonBeautyServiceCategories.Default);
        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public override async Task<SalonBeautyServiceCategoryDto> CreateAsync(CreateSalonBeautyServiceCategoryDto input)
    {
        await CheckCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Create, MultiTenancyPermissions.HostSalonBeautyServiceCategories.Create);

        var entity = new SalonBeautyServiceCategory
        {
            Name = input.Name,
            Description = input.Description,
            SortOrder = input.SortOrder,
            Status = input.Status,
            Note = input.Note
        };

        var created = await _repository.InsertAsync(entity, autoSave: true);
        return MapToDto(created);
    }

    public override async Task<SalonBeautyServiceCategoryDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceCategoryDto input)
    {
        await CheckCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Edit, MultiTenancyPermissions.HostSalonBeautyServiceCategories.Edit);

        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.SortOrder = input.SortOrder;
        entity.Status = input.Status;
        entity.Note = input.Note;

        var updated = await _repository.UpdateAsync(entity, autoSave: true);
        return MapToDto(updated);
    }

    public override async Task DeleteAsync(Guid id)
    {
        await CheckCategoryPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Delete, MultiTenancyPermissions.HostSalonBeautyServiceCategories.Delete);
        await _repository.DeleteAsync(id, autoSave: true);
    }

    private SalonBeautyServiceCategoryDto MapToDto(SalonBeautyServiceCategory entity)
    {
        return new SalonBeautyServiceCategoryDto
        {
            Id = entity.Id,
            Name = entity.Name,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            Status = entity.Status,
            Note = entity.Note
        };
    }

    private async Task CheckCategoryPolicyAsync(string tenantPermission, string hostPermission)
    {
        var permission = CurrentTenant.IsAvailable ? tenantPermission : hostPermission;
        if (permission.IsNullOrWhiteSpace())
            throw new AbpAuthorizationException("Missing Salon Beauty service category permission.");

        await AuthorizationService.CheckAsync(permission);
    }
}
