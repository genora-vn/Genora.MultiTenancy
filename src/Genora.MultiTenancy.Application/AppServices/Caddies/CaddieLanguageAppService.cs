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
public class CaddieLanguageAppService : ApplicationService
{
    private readonly IRepository<AppLanguage, Guid> _repo;
    private readonly ICurrentTenant _currentTenant;
    private readonly IGuidGenerator _guidGenerator;

    public CaddieLanguageAppService(
        IRepository<AppLanguage, Guid> repo,
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

    public async Task<PagedResultDto<CaddieLanguageDto>> GetListAsync(PagedAndSortedResultRequestDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppLanguages.Default, MultiTenancyPermissions.HostAppLanguages.Default));

        var query = await _repo.GetQueryableAsync();
        var totalCount = await AsyncExecuter.CountAsync(query);

        var sorting = input.Sorting.IsNullOrWhiteSpace() ? "SortOrder ASC" : input.Sorting;
        var items = await AsyncExecuter.ToListAsync(
            query.OrderBy(sorting).Skip(input.SkipCount).Take(input.MaxResultCount));

        var dtos = items.Select(x => new CaddieLanguageDto
        {
            Id = x.Id,
            LanguageCode = x.LanguageCode,
            LanguageName = x.LanguageName,
            NativeName = x.NativeName,
            Status = x.Status,
            SortOrder = x.SortOrder
        }).ToList();

        return new PagedResultDto<CaddieLanguageDto>(totalCount, dtos);
    }

    public async Task<ListResultDto<CaddieLanguageDto>> GetAllActiveAsync()
    {
        var query = (await _repo.GetQueryableAsync()).Where(x => x.Status == 1);
        var items = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.SortOrder));

        return new ListResultDto<CaddieLanguageDto>(items.Select(x => new CaddieLanguageDto
        {
            Id = x.Id,
            LanguageCode = x.LanguageCode,
            LanguageName = x.LanguageName,
            NativeName = x.NativeName,
            Status = x.Status,
            SortOrder = x.SortOrder
        }).ToList());
    }

    public async Task<CaddieLanguageDto> CreateAsync(CreateUpdateLanguageDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppLanguages.Create, MultiTenancyPermissions.HostAppLanguages.Create));

        var entity = new AppLanguage
        {
            LanguageCode = input.LanguageCode,
            LanguageName = input.LanguageName,
            NativeName = input.NativeName,
            Status = input.Status,
            SortOrder = input.SortOrder
        };

        await _repo.InsertAsync(entity, autoSave: true);

        return new CaddieLanguageDto
        {
            Id = entity.Id,
            LanguageCode = entity.LanguageCode,
            LanguageName = entity.LanguageName,
            NativeName = entity.NativeName,
            Status = entity.Status,
            SortOrder = entity.SortOrder
        };
    }

    public async Task<CaddieLanguageDto> UpdateAsync(Guid id, CreateUpdateLanguageDto input)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppLanguages.Edit, MultiTenancyPermissions.HostAppLanguages.Edit));

        var entity = await _repo.GetAsync(id);
        entity.LanguageCode = input.LanguageCode;
        entity.LanguageName = input.LanguageName;
        entity.NativeName = input.NativeName;
        entity.Status = input.Status;
        entity.SortOrder = input.SortOrder;

        await _repo.UpdateAsync(entity, autoSave: true);

        return new CaddieLanguageDto
        {
            Id = entity.Id,
            LanguageCode = entity.LanguageCode,
            LanguageName = entity.LanguageName,
            NativeName = entity.NativeName,
            Status = entity.Status,
            SortOrder = entity.SortOrder
        };
    }

    public async Task DeleteAsync(Guid id)
    {
        await AuthorizationService.CheckAsync(
            P(MultiTenancyPermissions.AppLanguages.Delete, MultiTenancyPermissions.HostAppLanguages.Delete));

        await _repo.DeleteAsync(id);
    }
}
