using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.SalonBeauty;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Authorization;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace Genora.MultiTenancy.Application.SalonBeauty;

[Authorize]
public class SalonBeautyServiceAppService : ApplicationService, ISalonBeautyServiceAppService
{
    private readonly IRepository<SalonBeautyService, Guid> _repository;

    public SalonBeautyServiceAppService(IRepository<SalonBeautyService, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<SalonBeautyServiceDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServices.Default);

        var query = await _repository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(),
            x => x.Name.Contains(input.FilterText));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyServiceDto>
        {
            TotalCount = totalCount,
            Items = items.Select(x => new SalonBeautyServiceDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryId = x.CategoryId,
                Price = x.Price,
                Duration = x.Duration,
                ApplicableRole = x.ApplicableRole,
                ApplicableLevel = x.ApplicableLevel,
                Status = x.Status,
                IsShowOnApp = x.IsShowOnApp,
                Note = x.Note,
                SortOrder = x.SortOrder
            }).ToList()
        };
    }

    public async Task<SalonBeautyServiceDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServices.Default);
        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<SalonBeautyServiceDto> CreateAsync(CreateSalonBeautyServiceDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServices.Create);

        var entity = new SalonBeautyService
        {
            Name = input.Name,
            CategoryId = input.CategoryId,
            Price = input.Price,
            Duration = input.Duration,
            ApplicableRole = input.ApplicableRole,
            ApplicableLevel = input.ApplicableLevel,
            Status = input.Status,
            IsShowOnApp = input.IsShowOnApp,
            Note = input.Note,
            SortOrder = input.SortOrder
        };

        var created = await _repository.InsertAsync(entity);
        return MapToDto(created);
    }

    public async Task<SalonBeautyServiceDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServices.Edit);

        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.CategoryId = input.CategoryId;
        entity.Price = input.Price;
        entity.Duration = input.Duration;
        entity.ApplicableRole = input.ApplicableRole;
        entity.ApplicableLevel = input.ApplicableLevel;
        entity.Status = input.Status;
        entity.IsShowOnApp = input.IsShowOnApp;
        entity.Note = input.Note;
        entity.SortOrder = input.SortOrder;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServices.Delete);
        await _repository.DeleteAsync(id);
    }

    private SalonBeautyServiceDto MapToDto(SalonBeautyService entity)
    {
        return new SalonBeautyServiceDto
        {
            Id = entity.Id,
            Name = entity.Name,
            CategoryId = entity.CategoryId,
            Price = entity.Price,
            Duration = entity.Duration,
            ApplicableRole = entity.ApplicableRole,
            ApplicableLevel = entity.ApplicableLevel,
            Status = entity.Status,
            IsShowOnApp = entity.IsShowOnApp,
            Note = entity.Note,
            SortOrder = entity.SortOrder
        };
    }

    private async Task CheckPolicyAsync(string permission)
        => await AuthorizationService.CheckAsync(permission);
}

[Authorize]
public class SalonBeautyServiceCategoryAppService : ApplicationService, ISalonBeautyServiceCategoryAppService
{
    private readonly IRepository<SalonBeautyServiceCategory, Guid> _repository;

    public SalonBeautyServiceCategoryAppService(IRepository<SalonBeautyServiceCategory, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<SalonBeautyServiceCategoryDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default);

        var query = await _repository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(),
            x => x.Name.Contains(input.FilterText));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyServiceCategoryDto>
        {
            TotalCount = totalCount,
            Items = items.Select(x => new SalonBeautyServiceCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                Description = x.Description,
                SortOrder = x.SortOrder,
                Status = x.Status,
                Note = x.Note
            }).ToList()
        };
    }

    public async Task<SalonBeautyServiceCategoryDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Default);
        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<SalonBeautyServiceCategoryDto> CreateAsync(CreateSalonBeautyServiceCategoryDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Create);

        var entity = new SalonBeautyServiceCategory
        {
            Name = input.Name,
            Description = input.Description,
            SortOrder = input.SortOrder,
            Status = input.Status,
            Note = input.Note
        };

        var created = await _repository.InsertAsync(entity);
        return MapToDto(created);
    }

    public async Task<SalonBeautyServiceCategoryDto> UpdateAsync(Guid id, UpdateSalonBeautyServiceCategoryDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Edit);

        var entity = await _repository.GetAsync(id);
        entity.Name = input.Name;
        entity.Description = input.Description;
        entity.SortOrder = input.SortOrder;
        entity.Status = input.Status;
        entity.Note = input.Note;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyServiceCategories.Delete);
        await _repository.DeleteAsync(id);
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

    private async Task CheckPolicyAsync(string permission)
        => await AuthorizationService.CheckAsync(permission);
}
