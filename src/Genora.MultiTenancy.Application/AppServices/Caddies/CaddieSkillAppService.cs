using System;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Caddies;
using Genora.MultiTenancy.DomainModels.AppCaddie;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.MultiTenancy;

namespace Genora.MultiTenancy.AppServices.Caddies;

[Authorize]
public class CaddieSkillAppService : ApplicationService
{
    private readonly IRepository<AppCaddieSkill, Guid> _repo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    public CaddieSkillAppService(
        IRepository<AppCaddieSkill, Guid> repo,
        ICurrentTenant currentTenant,
        IGuidGenerator guidGenerator)
    {
        _repo = repo;
        _currentTenant = currentTenant;
        _guidGenerator = guidGenerator;
        LocalizationResource = typeof(MultiTenancyResource);
    }

    private string P(string tenantPerm, string hostPerm)
        => _currentTenant.IsAvailable ? tenantPerm : hostPerm;

    public async Task<PagedResultDto<CaddieSkillDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSkills.Default, MultiTenancyPermissions.HostAppCaddieSkills.Default));

        var query = await _repo.GetQueryableAsync();
        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "SortOrder ASC" : input.Sorting;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = items.Select(x => new CaddieSkillDto
        {
            Id = x.Id,
            SkillCode = x.SkillCode,
            SkillName = x.SkillName,
            Description = x.Description,
            SortOrder = x.SortOrder,
            Status = x.Status
        }).ToList();

        return new PagedResultDto<CaddieSkillDto>(totalCount, dtos);
    }

    public async Task<ListResultDto<CaddieSkillDto>> GetAllActiveAsync()
    {
        var query = (await _repo.GetQueryableAsync()).Where(x => x.Status == 1);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder));

        return new ListResultDto<CaddieSkillDto>(items.Select(x => new CaddieSkillDto
        {
            Id = x.Id,
            SkillCode = x.SkillCode,
            SkillName = x.SkillName,
            Description = x.Description,
            SortOrder = x.SortOrder,
            Status = x.Status
        }).ToList());
    }

    public async Task<CaddieSkillDto> CreateAsync(CreateUpdateCaddieSkillDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSkills.Create, MultiTenancyPermissions.HostAppCaddieSkills.Create));

        var entity = new AppCaddieSkill
        {
            SkillCode = input.SkillCode,
            SkillName = input.SkillName,
            Description = input.Description,
            SortOrder = input.SortOrder,
            Status = input.Status
        };

        await _repo.InsertAsync(entity, autoSave: true);

        return new CaddieSkillDto
        {
            Id = entity.Id,
            SkillCode = entity.SkillCode,
            SkillName = entity.SkillName,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            Status = entity.Status
        };
    }

    public async Task<CaddieSkillDto> UpdateAsync(Guid id, CreateUpdateCaddieSkillDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSkills.Edit, MultiTenancyPermissions.HostAppCaddieSkills.Edit));

        var entity = await _repo.GetAsync(id);
        entity.SkillCode = input.SkillCode;
        entity.SkillName = input.SkillName;
        entity.Description = input.Description;
        entity.SortOrder = input.SortOrder;
        entity.Status = input.Status;

        await _repo.UpdateAsync(entity, autoSave: true);

        return new CaddieSkillDto
        {
            Id = entity.Id,
            SkillCode = entity.SkillCode,
            SkillName = entity.SkillName,
            Description = entity.Description,
            SortOrder = entity.SortOrder,
            Status = entity.Status
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppCaddieSkills.Delete, MultiTenancyPermissions.HostAppCaddieSkills.Delete));

        await _repo.DeleteAsync(id);
    }
}
