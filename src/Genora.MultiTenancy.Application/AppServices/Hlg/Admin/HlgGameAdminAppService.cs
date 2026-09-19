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
public class HlgGameAdminAppService : FeatureProtectedCrudAppService<HlgGame, HlgGameAdminDto, Guid, GetHlgAdminListInput, CreateHlgGameInput, UpdateHlgGameInput>, IHlgGameAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgGames.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgGames.Default;
    private readonly IRepository<HlgQuestion, Guid> _questions;
    private readonly IRepository<HlgGameSession, Guid> _sessions;
    public HlgGameAdminAppService(IRepository<HlgGame, Guid> repository, ICurrentTenant tenant, IFeatureChecker features, IRepository<HlgQuestion, Guid> questions, IRepository<HlgGameSession, Guid> sessions) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        _questions = questions;
        _sessions = sessions;
        GetPolicyName = MultiTenancyPermissions.AppHlgGames.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgGames.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgGames.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgGames.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgGames.Delete;
    }
    public override async Task<PagedResultDto<HlgGameAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Name.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlgGameAdminDto>(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgGameAdminDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return Map(await Repository.GetAsync(id));
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgGameAdminDto> CreateAsync(CreateHlgGameInput input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);
        var entity = new HlgGame(GuidGenerator.Create(), input.Name.Trim(), input.Type, CurrentTenant.Id);
        Apply(input, entity);
        await Repository.InsertAsync(entity, autoSave: true);
        return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgGameAdminDto> UpdateAsync(Guid id, UpdateHlgGameInput input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        await ValidateAsync(input, id);
        Apply(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        return Map(entity);
    }
    private async Task ValidateAsync(HlgGameInput input, Guid? id)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        if (id.HasValue && await _sessions.AnyAsync(x => x.GameId == id.Value))
        {
            var current = await Repository.GetAsync(id.Value);
            if (current.Type != input.Type || current.BaseScorePerQuestion != input.BaseScorePerQuestion)
                throw new UserFriendlyException(L["Hlg:GameHasSessions"]);
        }
        await Task.CompletedTask;
    }
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        if (await _questions.AnyAsync(x => x.GameId == id) || await _sessions.AnyAsync(x => x.GameId == id)) throw new UserFriendlyException(L["Hlg:GameHasChildren"]);
        await Repository.DeleteAsync(id);
    }
    private static void Apply(HlgGameInput input, HlgGame entity)
    {
        entity.Name = input.Name.Trim();
        entity.Type = input.Type;
        entity.ImageUrl = input.ImageUrl;
        entity.Description = input.Description;
        entity.Rules = input.Rules;
        entity.RewardDescription = input.RewardDescription;
        entity.Status = input.Status;
        entity.StartAt = input.StartAt;
        entity.EndAt = input.EndAt;
        entity.BaseScorePerQuestion = input.BaseScorePerQuestion;
        entity.DisplayOrder = input.DisplayOrder;
        entity.IsActive = input.IsActive;
    }
    private static HlgGameAdminDto Map(HlgGame entity) => new()
    {
        Id = entity.Id,
        Name = entity.Name,
        Type = entity.Type,
        ImageUrl = entity.ImageUrl,
        Description = entity.Description,
        Rules = entity.Rules,
        RewardDescription = entity.RewardDescription,
        Status = entity.Status,
        StartAt = entity.StartAt,
        EndAt = entity.EndAt,
        BaseScorePerQuestion = entity.BaseScorePerQuestion,
        DisplayOrder = entity.DisplayOrder,
        IsActive = entity.IsActive,
    };
}
