using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;
[Authorize]
public class HlgRankingAdminAppService : FeatureProtectedCrudAppService<HlgRankingEvent, HlgRankingAdminDto, Guid, GetHlgAdminListInput, CreateHlgRankingInput, UpdateHlgRankingInput>, IHlgRankingAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgRanking.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgRanking.Default;
    public HlgRankingAdminAppService(IRepository<HlgRankingEvent, Guid> repository, ICurrentTenant tenant, IFeatureChecker features) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        GetPolicyName = MultiTenancyPermissions.AppHlgRanking.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgRanking.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgRanking.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgRanking.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgRanking.Delete;
    }
    public override async Task<PagedResultDto<HlgRankingAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Title.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderByDescending(x => x.StartAt).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlgRankingAdminDto>(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgRankingAdminDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return Map(await Repository.GetAsync(id));
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgRankingAdminDto> CreateAsync(CreateHlgRankingInput input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);
        var entity = new HlgRankingEvent(GuidGenerator.Create(), input.Title.Trim(), input.StartAt, input.EndAt, CurrentTenant.Id);
        Apply(input, entity);
        await Repository.InsertAsync(entity, autoSave: true);
        return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgRankingAdminDto> UpdateAsync(Guid id, UpdateHlgRankingInput input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        await ValidateAsync(input, id);
        Apply(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        return Map(entity);
    }
    private async Task ValidateAsync(HlgRankingInput input, Guid? id)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        await Task.CompletedTask;
    }
    private static void Apply(HlgRankingInput input, HlgRankingEvent entity)
    {
        entity.Title = input.Title.Trim();
        entity.Description = input.Description;
        entity.StartAt = input.StartAt;
        entity.EndAt = input.EndAt;
        entity.IsActive = input.IsActive;
    }
    private static HlgRankingAdminDto Map(HlgRankingEvent entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        StartAt = entity.StartAt,
        EndAt = entity.EndAt,
        IsActive = entity.IsActive,
    };
}
