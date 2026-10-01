using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;
using Volo.Abp.Content;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
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
    private readonly HlgRankingResultExcelExporter _excelExporter;
    private readonly IRepository<HlgRankingResultSnapshot, Guid> _resultSnapshots;
    public HlgRankingAdminAppService(
        IRepository<HlgRankingEvent, Guid> repository,
        ICurrentTenant tenant,
        IFeatureChecker features,
        HlgRankingResultExcelExporter excelExporter,
        IRepository<HlgRankingResultSnapshot, Guid> resultSnapshots) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        GetPolicyName = MultiTenancyPermissions.AppHlgRanking.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgRanking.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgRanking.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgRanking.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgRanking.Delete;
        _excelExporter = excelExporter;
        _resultSnapshots = resultSnapshots;
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
        if (input.GameId.HasValue) {
            var game = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame,Guid>>().GetAsync(input.GameId.Value);
            HlgContentValidation.Scope(game.TenantId,CurrentTenant.Id);
        }
        if (id.HasValue && await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgRankingWinner,Guid>>().AnyAsync(x => x.EventId == id)) {
            var old = await Repository.GetAsync(id.Value);
            if (old.GameId != input.GameId || old.StartAt != input.StartAt || old.EndAt != input.EndAt) throw new UserFriendlyException(L["Hlg:EventHasWinners"]);
        }
        if (id.HasValue && await _resultSnapshots.AnyAsync(x => x.EventId == id.Value)) {
            var old = await Repository.GetAsync(id.Value);
            if (old.GameId != input.GameId || old.StartAt != input.StartAt || old.EndAt != input.EndAt) throw new UserFriendlyException(L["Hlg:EventResultsSnapshotted"]);
        }
    }
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync();
        if (await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgRankingPrize,Guid>>().AnyAsync(x => x.EventId == id)) throw new UserFriendlyException(L["Hlg:EventHasPrizes"]);
        if (await _resultSnapshots.AnyAsync(x => x.EventId == id)) throw new UserFriendlyException(L["Hlg:EventResultsSnapshotted"]);
        await Repository.DeleteAsync(id);
    }
    [UnitOfWork(isTransactional: true)]
    public async Task<IRemoteStreamContent> ExportResultsAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var rankingEvent = await Repository.GetAsync(id);
        HlgContentValidation.Scope(rankingEvent.TenantId, CurrentTenant.Id);
        if (Clock.Now <= rankingEvent.EndAt) throw new UserFriendlyException(L["Hlg:EventNotEndedForExport"]);

        var savedSnapshots = await AsyncExecuter.ToListAsync(
            (await _resultSnapshots.GetQueryableAsync())
                .Where(x => x.TenantId == CurrentTenant.Id && x.EventId == id)
                .OrderBy(x => x.EventRank).ThenBy(x => x.PlayerName).ThenBy(x => x.GameName));
        if (savedSnapshots.Count > 0)
            return _excelExporter.Export(rankingEvent.Title, savedSnapshots.Select(MapSnapshot).ToList());

        var sessionQuery = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGameSession, Guid>>().GetQueryableAsync();
        var sessions = await AsyncExecuter.ToListAsync(sessionQuery.Where(x =>
            x.TenantId == CurrentTenant.Id && x.IsFinished && x.FinishedAt.HasValue
            && x.FinishedAt >= rankingEvent.StartAt && x.FinishedAt <= rankingEvent.EndAt
            && (!rankingEvent.GameId.HasValue || x.GameId == rankingEvent.GameId.Value)));

        if (sessions.Count == 0) return _excelExporter.Export(rankingEvent.Title, Array.Empty<HlgRankingResultExcelRow>());

        var customerIds = sessions.Select(x => x.CustomerId).Distinct().ToList();
        var gameIds = sessions.Select(x => x.GameId).Distinct().ToList();
        var customers = await AsyncExecuter.ToListAsync(
            (await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().GetQueryableAsync())
                .Where(x => x.TenantId == CurrentTenant.Id && customerIds.Contains(x.Id)));
        var games = await AsyncExecuter.ToListAsync(
            (await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame, Guid>>().GetQueryableAsync())
                .Where(x => x.TenantId == CurrentTenant.Id && gameIds.Contains(x.Id)));
        var customerById = customers.ToDictionary(x => x.Id);
        var gameById = games.ToDictionary(x => x.Id);
        var eventScores = sessions.GroupBy(x => x.CustomerId).ToDictionary(x => x.Key, x => x.Sum(y => y.Score));
        var ranks = eventScores.OrderByDescending(x => x.Value).ThenBy(x => x.Key)
            .Select((x, index) => new { x.Key, Rank = index + 1 }).ToDictionary(x => x.Key, x => x.Rank);

        var rows = sessions.GroupBy(x => new { x.CustomerId, x.GameId })
            .Select(group =>
            {
                customerById.TryGetValue(group.Key.CustomerId, out var customer);
                gameById.TryGetValue(group.Key.GameId, out var game);
                return new HlgRankingResultExcelRow
                {
                    CustomerId = group.Key.CustomerId,
                    GameId = group.Key.GameId,
                    EventRank = ranks[group.Key.CustomerId],
                    CustomerCode = customer?.CustomerCode,
                    PlayerName = customer?.FullName ?? "Người chơi",
                    PhoneNumber = customer?.PhoneNumber ?? "",
                    ZaloUserId = customer?.ZaloUserId,
                    GameName = game?.Name ?? "Trò chơi đã xóa",
                    PlayCount = group.Count(),
                    GameScore = group.Sum(x => x.Score),
                    BestScore = group.Max(x => x.Score),
                    CorrectAnswerCount = group.Sum(x => x.CorrectCount),
                    TotalQuestionCount = group.Sum(x => x.TotalQuestions),
                    EventScore = eventScores[group.Key.CustomerId],
                    FirstPlayedAt = group.Min(x => x.FinishedAt)!.Value,
                    LastPlayedAt = group.Max(x => x.FinishedAt)!.Value
                };
            })
            .OrderBy(x => x.EventRank).ThenBy(x => x.PlayerName).ThenBy(x => x.GameName).ToList();

        if (rows.Count > 0)
        {
            var snapshots = rows.Select(row => new HlgRankingResultSnapshot(
                GuidGenerator.Create(), rankingEvent.Id, row.CustomerId, row.GameId, CurrentTenant.Id)
            {
                EventRank = row.EventRank,
                CustomerCode = row.CustomerCode,
                PlayerName = row.PlayerName,
                PhoneNumber = row.PhoneNumber,
                ZaloUserId = row.ZaloUserId,
                GameName = row.GameName,
                PlayCount = row.PlayCount,
                GameScore = row.GameScore,
                BestScore = row.BestScore,
                CorrectAnswerCount = row.CorrectAnswerCount,
                TotalQuestionCount = row.TotalQuestionCount,
                EventScore = row.EventScore,
                FirstPlayedAt = row.FirstPlayedAt,
                LastPlayedAt = row.LastPlayedAt
            }).ToList();
            await _resultSnapshots.InsertManyAsync(snapshots, autoSave: true);

            foreach (var customer in customers) customer.BonusPoint = 0;
            await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>()
                .UpdateManyAsync(customers, autoSave: true);
        }

        return _excelExporter.Export(rankingEvent.Title, rows);
    }

    private static HlgRankingResultExcelRow MapSnapshot(HlgRankingResultSnapshot snapshot) => new()
    {
        CustomerId = snapshot.CustomerId,
        GameId = snapshot.GameId,
        EventRank = snapshot.EventRank,
        CustomerCode = snapshot.CustomerCode,
        PlayerName = snapshot.PlayerName,
        PhoneNumber = snapshot.PhoneNumber,
        ZaloUserId = snapshot.ZaloUserId,
        GameName = snapshot.GameName,
        PlayCount = snapshot.PlayCount,
        GameScore = snapshot.GameScore,
        BestScore = snapshot.BestScore,
        CorrectAnswerCount = snapshot.CorrectAnswerCount,
        TotalQuestionCount = snapshot.TotalQuestionCount,
        EventScore = snapshot.EventScore,
        FirstPlayedAt = snapshot.FirstPlayedAt,
        LastPlayedAt = snapshot.LastPlayedAt
    };
    private static void Apply(HlgRankingInput input, HlgRankingEvent entity)
    {
        entity.Title = input.Title.Trim();
        entity.Description = input.Description;
        entity.StartAt = input.StartAt;
        entity.EndAt = input.EndAt;
        entity.GameId = input.GameId;
        entity.IsActive = input.IsActive;
    }
    private HlgRankingAdminDto Map(HlgRankingEvent entity) => new()
    {
        Id = entity.Id,
        Title = entity.Title,
        Description = entity.Description,
        StartAt = entity.StartAt,
        EndAt = entity.EndAt,
        GameId = entity.GameId,
        IsActive = entity.IsActive,
        CanExportResults = Clock.Now > entity.EndAt,
    };
}
