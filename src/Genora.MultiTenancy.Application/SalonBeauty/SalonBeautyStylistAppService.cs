using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.SalonBeautyDtos;
using Genora.MultiTenancy.DomainModels.AppSalonBeauty;
using Genora.MultiTenancy.Permissions;
using Genora.MultiTenancy.SalonBeauty;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Linq;

namespace Genora.MultiTenancy.Application.SalonBeauty;

[Authorize]
public class SalonBeautyStylistAppService : ApplicationService, ISalonBeautyStylistAppService
{
    private readonly IRepository<SalonBeautyStylist, Guid> _repository;

    public SalonBeautyStylistAppService(IRepository<SalonBeautyStylist, Guid> repository)
    {
        _repository = repository;
    }

    public async Task<PagedResultDto<SalonBeautyStylistDto>> GetListAsync(GetSalonBeautyListInput input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyStylists.Default);

        var query = await _repository.GetQueryableAsync();
        query = query.WhereIf(!input.FilterText.IsNullOrWhiteSpace(),
            x => x.DisplayName.Contains(input.FilterText));

        var totalCount = await AsyncExecuter.CountAsync(query);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder).Skip(input.SkipCount).Take(input.MaxResultCount));

        return new PagedResultDto<SalonBeautyStylistDto>
        {
            TotalCount = totalCount,
            Items = items.Select(MapToDto).ToList()
        };
    }

    public async Task<SalonBeautyStylistDto> GetAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyStylists.Default);
        var entity = await _repository.GetAsync(id);
        return MapToDto(entity);
    }

    public async Task<SalonBeautyStylistDto> CreateAsync(CreateSalonBeautyStylistDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyStylists.Create);

        var entity = new SalonBeautyStylist
        {
            DisplayName = input.DisplayName,
            Avatar = input.Avatar,
            Phone = input.Phone,
            Gender = input.Gender,
            Role = input.Role,
            Level = input.Level,
            ExperienceYear = input.ExperienceYear,
            Status = input.Status,
            IsShowOnApp = input.IsShowOnApp,
            Note = input.Note,
            SortOrder = input.SortOrder
        };

        var created = await _repository.InsertAsync(entity);
        return MapToDto(created);
    }

    public async Task<SalonBeautyStylistDto> UpdateAsync(Guid id, UpdateSalonBeautyStylistDto input)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyStylists.Edit);

        var entity = await _repository.GetAsync(id);
        entity.DisplayName = input.DisplayName;
        entity.Avatar = input.Avatar;
        entity.Phone = input.Phone;
        entity.Gender = input.Gender;
        entity.Role = input.Role;
        entity.Level = input.Level;
        entity.ExperienceYear = input.ExperienceYear;
        entity.Status = input.Status;
        entity.IsShowOnApp = input.IsShowOnApp;
        entity.Note = input.Note;
        entity.SortOrder = input.SortOrder;

        var updated = await _repository.UpdateAsync(entity);
        return MapToDto(updated);
    }

    public async Task DeleteAsync(Guid id)
    {
        await CheckPolicyAsync(MultiTenancyPermissions.SalonBeautyStylists.Delete);
        await _repository.DeleteAsync(id);
    }

    private SalonBeautyStylistDto MapToDto(SalonBeautyStylist entity)
    {
        return new SalonBeautyStylistDto
        {
            Id = entity.Id,
            DisplayName = entity.DisplayName,
            Avatar = entity.Avatar,
            Phone = entity.Phone,
            Gender = entity.Gender,
            Role = entity.Role,
            Level = entity.Level,
            ExperienceYear = entity.ExperienceYear,
            RatingAvg = entity.RatingAvg,
            TotalBooking = entity.TotalBooking,
            Status = entity.Status,
            IsShowOnApp = entity.IsShowOnApp,
            Note = entity.Note,
            SortOrder = entity.SortOrder
        };
    }

    private async Task CheckPolicyAsync(string permission)
        => await AuthorizationService.CheckAsync(permission);
}
