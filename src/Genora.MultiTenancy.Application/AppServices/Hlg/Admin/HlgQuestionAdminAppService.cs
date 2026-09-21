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
public class HlgQuestionAdminAppService : FeatureProtectedCrudAppService<HlgQuestion, HlgQuestionAdminDto, Guid, GetHlgAdminListInput, CreateHlgQuestionInput, UpdateHlgQuestionInput>, IHlgQuestionAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgGames.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgGames.Default;
    private readonly IRepository<HlgGame, Guid> _games;
    private readonly IRepository<HlgAnswerOption, Guid> _options;
    private readonly IRepository<HlgGameSession, Guid> _sessions;
    public HlgQuestionAdminAppService(IRepository<HlgQuestion, Guid> repository, ICurrentTenant tenant, IFeatureChecker features, IRepository<HlgGame, Guid> games, IRepository<HlgAnswerOption, Guid> options, IRepository<HlgGameSession, Guid> sessions) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        _games = games;
        _options = options;
        _sessions = sessions;
        GetPolicyName = MultiTenancyPermissions.AppHlgGames.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgGames.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgGames.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgGames.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgGames.Delete;
    }
    public override async Task<PagedResultDto<HlgQuestionAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = await Repository.GetQueryableAsync();
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Content.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive.Value);
        if (input.ParentId.HasValue) query = query.Where(x => x.GameId == input.ParentId.Value);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Index).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        return new PagedResultDto<HlgQuestionAdminDto>(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgQuestionAdminDto> GetAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        return Map(await Repository.GetAsync(id));
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgQuestionAdminDto> CreateAsync(CreateHlgQuestionInput input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);
        var entity = new HlgQuestion(GuidGenerator.Create(), input.GameId, input.Index, input.Content.Trim(), CurrentTenant.Id);
        Apply(input, entity);
        await Repository.InsertAsync(entity, autoSave: true);
        await SaveOptionsAsync(entity.Id, input);
        return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgQuestionAdminDto> UpdateAsync(Guid id, UpdateHlgQuestionInput input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        await ValidateAsync(input, id);
        if (entity.GameId != input.GameId) throw new UserFriendlyException(L["Hlg:ParentCannotChange"]);
        Apply(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        await SaveOptionsAsync(entity.Id, input);
        return Map(entity);
    }
    private async Task ValidateAsync(HlgQuestionInput input, Guid? id)
    {
        Validator.ValidateObject(input, new ValidationContext(input), true);
        HlgContentValidation.Localized(() => HlgContentValidation.Url(input.ImageUrl), key => L[key]);
        await _games.GetAsync(input.GameId);
        if (await _sessions.AnyAsync(x => x.GameId == input.GameId)) throw new UserFriendlyException(L["Hlg:GameHasSessions"]);
        if (await Repository.AnyAsync(x => x.GameId == input.GameId && x.Index == input.Index && x.Id != id)) throw new UserFriendlyException(L["Hlg:QuestionIndexExists"]);
        await Task.CompletedTask;
    }
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        var question = await Repository.GetAsync(id);
        if (await _sessions.AnyAsync(x => x.GameId == question.GameId)) throw new UserFriendlyException(L["Hlg:GameHasSessions"]);
        await Repository.DeleteAsync(id);
    }
    private static void Apply(HlgQuestionInput input, HlgQuestion entity)
    {
        entity.GameId = input.GameId;
        entity.Index = input.Index;
        entity.Content = input.Content.Trim();
        entity.ImageUrl = input.ImageUrl;
        entity.TimeLimitSec = input.TimeLimitSec;
        entity.ScoreMultiplier = input.ScoreMultiplier;
        entity.IsActive = input.IsActive;
        entity.CorrectKey = input.CorrectKey;
    }
    private static HlgQuestionAdminDto Map(HlgQuestion entity) => new()
    {
        Id = entity.Id,
        GameId = entity.GameId,
        Index = entity.Index,
        Content = entity.Content,
        ImageUrl = entity.ImageUrl,
        TimeLimitSec = entity.TimeLimitSec,
        ScoreMultiplier = entity.ScoreMultiplier,
        IsActive = entity.IsActive,
    };
    // Secret answer is available ONLY to editors, never on list/get or mini-app DTOs.
    public virtual async Task<HlgQuestionInput> GetEditorAsync(Guid id)
    {
        await CheckUpdatePolicyAsync();
        var e = await Repository.GetAsync(id);
        var options = await _options.GetListAsync(x => x.QuestionId == id);
        string? Option(HlgAnswerKey key) => options.FirstOrDefault(x => x.Key == key)?.Content;
        return new HlgQuestionInput { GameId = e.GameId, Index = e.Index, Content = e.Content, ImageUrl = e.ImageUrl,
            TimeLimitSec = e.TimeLimitSec, ScoreMultiplier = e.ScoreMultiplier, CorrectKey = e.CorrectKey, IsActive = e.IsActive,
            OptionA = Option(HlgAnswerKey.A) ?? "", OptionB = Option(HlgAnswerKey.B) ?? "", OptionC = Option(HlgAnswerKey.C), OptionD = Option(HlgAnswerKey.D) };
    }
    private async Task SaveOptionsAsync(Guid id, HlgQuestionInput input)
    {
        var existing = await _options.GetListAsync(x => x.QuestionId == id);
        var texts = new[] { input.OptionA, input.OptionB, input.OptionC, input.OptionD };
        for (var i = 0; i < texts.Length; i++)
        {
            var key = (HlgAnswerKey)(i + 1);
            var option = existing.FirstOrDefault(x => x.Key == key);
            if (string.IsNullOrWhiteSpace(texts[i])) { if (option != null) await _options.DeleteAsync(option, autoSave: true); continue; }
            if (option == null) await _options.InsertAsync(new HlgAnswerOption(GuidGenerator.Create(), id, key, texts[i]!.Trim(), CurrentTenant.Id), autoSave: true);
            else { option.Content = texts[i]!.Trim(); await _options.UpdateAsync(option, autoSave: true); }
        }
    }
}
