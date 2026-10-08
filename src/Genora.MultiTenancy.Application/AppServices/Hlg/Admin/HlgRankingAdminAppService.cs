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
    private readonly IRepository<HlgRankingEventGame, Guid> _eventGames;
    public HlgRankingAdminAppService(
        IRepository<HlgRankingEvent, Guid> repository,
        ICurrentTenant tenant,
        IFeatureChecker features,
        HlgRankingResultExcelExporter excelExporter,
        IRepository<HlgRankingResultSnapshot, Guid> resultSnapshots,
        IRepository<HlgRankingEventGame, Guid> eventGames) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        GetPolicyName = MultiTenancyPermissions.AppHlgRanking.Default;
        GetListPolicyName = MultiTenancyPermissions.AppHlgRanking.Default;
        CreatePolicyName = MultiTenancyPermissions.AppHlgRanking.Create;
        UpdatePolicyName = MultiTenancyPermissions.AppHlgRanking.Edit;
        DeletePolicyName = MultiTenancyPermissions.AppHlgRanking.Delete;
        _excelExporter = excelExporter;
        _resultSnapshots = resultSnapshots;
        _eventGames = eventGames;
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
        var dto = Map(await Repository.GetAsync(id));
        dto.GameIds = await AsyncExecuter.ToListAsync(
            (await _eventGames.GetQueryableAsync())
                .Where(x => x.TenantId == CurrentTenant.Id && x.EventId == id)
                .OrderBy(x => x.DisplayOrder).Select(x => x.GameId));
        return dto;
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgRankingAdminDto> CreateAsync(CreateHlgRankingInput input)
    {
        await CheckCreatePolicyAsync();
        await ValidateAsync(input, null);
        var entity = new HlgRankingEvent(GuidGenerator.Create(), input.Title.Trim(), input.StartAt, input.EndAt, CurrentTenant.Id);
        Apply(input, entity);
        await Repository.InsertAsync(entity, autoSave: true);
        var dto = Map(entity);
        dto.GameIds = await SyncEventGamesAsync(entity.Id, input.GameIds);
        return dto;
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgRankingAdminDto> UpdateAsync(Guid id, UpdateHlgRankingInput input)
    {
        await CheckUpdatePolicyAsync();
        var entity = await Repository.GetAsync(id);
        await ValidateAsync(input, id);
        Apply(input, entity);
        await Repository.UpdateAsync(entity, autoSave: true);
        var dto = Map(entity);
        dto.GameIds = await SyncEventGamesAsync(entity.Id, input.GameIds);
        return dto;
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

        // Các game cố định của chiến dịch (map campaign↔games). Rỗng = tất cả game (legacy).
        var campaignGameIds = await GetCampaignGameIdsAsync(id, rankingEvent.GameId);

        var savedSnapshots = await AsyncExecuter.ToListAsync(
            (await _resultSnapshots.GetQueryableAsync())
                .Where(x => x.TenantId == CurrentTenant.Id && x.EventId == id)
                .OrderBy(x => x.EventRank).ThenBy(x => x.PlayerName).ThenBy(x => x.GameName));
        if (savedSnapshots.Count > 0)
        {
            var snapshotRows = savedSnapshots.Select(MapSnapshot).ToList();
            await EnrichExportRowsAsync(rankingEvent, snapshotRows, campaignGameIds);
            return _excelExporter.Export(rankingEvent.Title, snapshotRows);
        }

        var sessionQuery = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGameSession, Guid>>().GetQueryableAsync();
        var sessions = await AsyncExecuter.ToListAsync(sessionQuery.Where(x =>
            x.TenantId == CurrentTenant.Id && x.IsFinished && x.FinishedAt.HasValue
            && x.FinishedAt >= rankingEvent.StartAt && x.FinishedAt <= rankingEvent.EndAt
            && (campaignGameIds.Count == 0 || campaignGameIds.Contains(x.GameId))));

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
        // BD: điểm xếp hạng CHỈ tính từ phiên ĐẠT (hoàn thành đủ điều kiện) — bỏ qua phiên thất bại
        // để không thổi phồng tổng điểm/thứ hạng. eventScores keyed theo mọi khách (0 nếu chưa đạt game nào).
        var eventScores = sessions.GroupBy(x => x.CustomerId)
            .ToDictionary(g => g.Key, g => g.Where(IsFinishedSessionPassed).Sum(y => y.Score));
        var ranks = eventScores.OrderByDescending(x => x.Value).ThenBy(x => x.Key)
            .Select((x, index) => new { x.Key, Rank = index + 1 }).ToDictionary(x => x.Key, x => x.Rank);

        var rows = sessions.GroupBy(x => new { x.CustomerId, x.GameId })
            .Select(group =>
            {
                customerById.TryGetValue(group.Key.CustomerId, out var customer);
                gameById.TryGetValue(group.Key.GameId, out var game);
                var passed = group.Where(IsFinishedSessionPassed).ToList(); // chỉ phiên ĐẠT mới tính điểm
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
                    PlayCount = group.Count(),                                   // tổng lượt chơi (gồm cả thất bại)
                    GameScore = passed.Sum(x => x.Score),                        // điểm chỉ từ phiên ĐẠT
                    BestScore = passed.Count > 0 ? passed.Max(x => x.Score) : 0, // điểm phiên tốt nhất đã ĐẠT
                    CorrectAnswerCount = passed.Sum(x => x.CorrectCount),
                    TotalQuestionCount = passed.Sum(x => x.TotalQuestions),
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

        await EnrichExportRowsAsync(rankingEvent, rows, campaignGameIds);
        return _excelExporter.Export(rankingEvent.Title, rows);
    }

    /// <summary>
    /// Bổ sung 3 cột phục vụ vận hành vào các dòng kết quả (dùng dữ liệu LIVE để luôn cập nhật,
    /// kể cả khi đã có snapshot): Quà nhận được (winner), Đã tham gia x/n, Địa chỉ nhận quà.
    /// </summary>
    private async Task EnrichExportRowsAsync(HlgRankingEvent ev, List<HlgRankingResultExcelRow> rows, List<Guid> campaignGameIds)
    {
        if (rows.Count == 0) return;
        var customerIds = rows.Select(r => r.CustomerId).Distinct().ToList();

        // (1) Quà nhận được: khách đã được gán quà (winner còn hiệu lực) trong sự kiện này.
        var rewardedCustomerIds = (await AsyncExecuter.ToListAsync(
            (await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgRankingWinner, Guid>>().GetQueryableAsync())
                .Where(w => w.TenantId == CurrentTenant.Id && w.EventId == ev.Id && w.IsActive
                            && customerIds.Contains(w.CustomerId))
                .Select(w => w.CustomerId).Distinct())).ToHashSet();

        // (2) Địa chỉ nhận quà: Customer.Address.
        var addressByCustomer = (await AsyncExecuter.ToListAsync(
            (await LazyServiceProvider.LazyGetRequiredService<IRepository<Customer, Guid>>().GetQueryableAsync())
                .Where(c => c.TenantId == CurrentTenant.Id && customerIds.Contains(c.Id))
                .Select(c => new { c.Id, c.Address })))
            .ToDictionary(x => x.Id, x => x.Address);

        // (3) Đã tham gia x/n: n = số game của chiến dịch (có phiên hoàn thành trong kỳ);
        //     x = số game khách HOÀN THÀNH đủ điều kiện (đúng cấu hình số câu sai cho phép).
        var sessions = await AsyncExecuter.ToListAsync(
            (await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGameSession, Guid>>().GetQueryableAsync())
                .Where(s => s.TenantId == CurrentTenant.Id && s.IsFinished && s.FinishedAt.HasValue
                            && s.FinishedAt >= ev.StartAt && s.FinishedAt <= ev.EndAt
                            && (campaignGameIds.Count == 0 || campaignGameIds.Contains(s.GameId))));

        // Mẫu số n: ưu tiên số game cố định của chiến dịch; nếu "tất cả game" thì dùng số game có phiên trong kỳ.
        var campaignGameCount = campaignGameIds.Count > 0
            ? campaignGameIds.Count
            : sessions.Select(s => s.GameId).Distinct().Count();
        var passedGamesByCustomer = sessions
            .Where(IsFinishedSessionPassed)
            .GroupBy(s => s.CustomerId)
            .ToDictionary(g => g.Key, g => g.Select(s => s.GameId).Distinct().Count());

        foreach (var row in rows)
        {
            row.RewardReceived = rewardedCustomerIds.Contains(row.CustomerId);
            row.RewardAddress = addressByCustomer.TryGetValue(row.CustomerId, out var addr) ? addr : null;
            row.GamesCompleted = passedGamesByCustomer.TryGetValue(row.CustomerId, out var done) ? done : 0;
            row.GamesInCampaign = campaignGameCount;
        }
    }

    /// <summary>
    /// Phiên đã kết thúc được xem là HOÀN THÀNH đủ điều kiện khi số câu sai (TotalQuestions - CorrectCount)
    /// không vượt mức cho phép của phiên (null = không giới hạn). Đồng nhất với luật ở HlgGameAppService.
    /// </summary>
    private static bool IsFinishedSessionPassed(HlgGameSession s)
    {
        if (s.TotalQuestions <= 0) return false;
        var wrong = s.TotalQuestions - s.CorrectCount;
        return !(s.AllowedWrongAnswers.HasValue && wrong > s.AllowedWrongAnswers.Value);
    }

    /// <summary>
    /// Đồng bộ danh sách game cố định của chiến dịch vào bảng map (xóa cái không còn, thêm cái mới).
    /// Trả về danh sách gameId đã lưu (đã chuẩn hóa).
    /// </summary>
    private async Task<List<Guid>> SyncEventGamesAsync(Guid eventId, List<Guid>? gameIds)
    {
        var desired = (gameIds ?? new List<Guid>()).Where(g => g != Guid.Empty).Distinct().ToList();
        var existing = await AsyncExecuter.ToListAsync(
            (await _eventGames.GetQueryableAsync()).Where(x => x.EventId == eventId));
        var existingIds = existing.Select(x => x.GameId).ToList();

        var toRemove = existing.Where(x => !desired.Contains(x.GameId)).ToList();
        if (toRemove.Count > 0) await _eventGames.DeleteManyAsync(toRemove, autoSave: true);

        var toAdd = desired.Where(g => !existingIds.Contains(g)).ToList();
        if (toAdd.Count > 0)
            await _eventGames.InsertManyAsync(
                toAdd.Select((g, i) => new HlgRankingEventGame(GuidGenerator.Create(), eventId, g, CurrentTenant.Id) { DisplayOrder = i }).ToList(),
                autoSave: true);
        return desired;
    }

    /// <summary>
    /// Danh sách game của chiến dịch dùng khi xuất Excel: ưu tiên bảng map; nếu rỗng dùng GameId đơn (legacy);
    /// rỗng = chiến dịch "tất cả game" (không giới hạn).
    /// Phòng mapping/GameId legacy bị "treo" (trỏ tới game đã xóa, ví dụ do xóa dữ liệu test): nếu KHÔNG còn
    /// game nào trong danh sách thực sự tồn tại, coi như "không giới hạn" để tránh chặn oan toàn bộ chức năng.
    /// </summary>
    private async Task<List<Guid>> GetCampaignGameIdsAsync(Guid eventId, Guid? legacyGameId)
    {
        var mapped = await AsyncExecuter.ToListAsync(
            (await _eventGames.GetQueryableAsync())
                .Where(x => x.TenantId == CurrentTenant.Id && x.EventId == eventId)
                .Select(x => x.GameId));
        var gameIds = mapped.Count > 0 ? mapped.Distinct().ToList() : (legacyGameId.HasValue ? new List<Guid> { legacyGameId.Value } : new List<Guid>());
        if (gameIds.Count > 0)
        {
            var gameQ = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame, Guid>>().GetQueryableAsync();
            var existingCount = await AsyncExecuter.CountAsync(gameQ.Where(g => g.TenantId == CurrentTenant.Id && gameIds.Contains(g.Id)));
            if (existingCount == 0) gameIds = new List<Guid>();
        }
        return gameIds;
    }

    /// <summary>
    /// (A) XUẤT BÁO CÁO: xuất kết quả các game ĐÃ KẾT THÚC (EndAt &lt; nay) thuộc chiến dịch — xem bất kỳ lúc nào.
    /// KHÔNG đóng băng snapshot, KHÔNG reset điểm (khác với "Chốt kết quả sự kiện" = ExportResultsAsync).
    /// gameIds rỗng = xuất toàn bộ game đã kết thúc của sự kiện.
    /// </summary>
    [UnitOfWork]
    public async Task<IRemoteStreamContent> ExportReportAsync(Guid id, List<Guid> gameIds)
    {
        await CheckGetPolicyAsync();
        var ev = await Repository.GetAsync(id);
        HlgContentValidation.Scope(ev.TenantId, CurrentTenant.Id);

        var endedGameIds = await GetEndedCampaignGameIdsAsync(ev);
        var selected = (gameIds == null || gameIds.Count == 0)
            ? endedGameIds
            : gameIds.Where(endedGameIds.Contains).ToList();
        if (selected.Count == 0) throw new UserFriendlyException(L["Hlg:NoEndedGameToExport"]);

        var sessionQuery = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGameSession, Guid>>().GetQueryableAsync();
        var sessions = await AsyncExecuter.ToListAsync(sessionQuery.Where(x =>
            x.TenantId == CurrentTenant.Id && x.IsFinished && x.FinishedAt.HasValue
            && x.FinishedAt >= ev.StartAt && x.FinishedAt <= ev.EndAt
            && selected.Contains(x.GameId)));

        var rows = await BuildRowsAsync(sessions);
        await EnrichExportRowsAsync(ev, rows, selected);
        return _excelExporter.Export(ev.Title + " - bao cao", rows);
    }

    /// <summary>Danh sách game ĐÃ KẾT THÚC (EndAt &lt; nay) thuộc chiến dịch — cho modal chọn game để xuất báo cáo.</summary>
    public async Task<List<HlgEndedGameDto>> GetEndedCampaignGamesAsync(Guid id)
    {
        await CheckGetPolicyAsync();
        var ev = await Repository.GetAsync(id);
        HlgContentValidation.Scope(ev.TenantId, CurrentTenant.Id);
        var campaignGameIds = await GetCampaignGameIdsAsync(id, ev.GameId);
        var now = Clock.Now;
        var gameQ = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame, Guid>>().GetQueryableAsync();
        var q = gameQ.Where(g => g.TenantId == CurrentTenant.Id && g.EndAt != null && g.EndAt < now);
        if (campaignGameIds.Count > 0) q = q.Where(g => campaignGameIds.Contains(g.Id));
        var games = await AsyncExecuter.ToListAsync(q.OrderBy(g => g.Name).Select(g => new { g.Id, g.Name, g.EndAt }));
        return games.Select(g => new HlgEndedGameDto { Id = g.Id, Name = g.Name, EndAt = g.EndAt }).ToList();
    }

    /// <summary>Id các game đã kết thúc thuộc chiến dịch (dùng để giới hạn phạm vi xuất báo cáo).</summary>
    private async Task<List<Guid>> GetEndedCampaignGameIdsAsync(HlgRankingEvent ev)
    {
        var campaignGameIds = await GetCampaignGameIdsAsync(ev.Id, ev.GameId);
        var now = Clock.Now;
        var gameQ = await LazyServiceProvider.LazyGetRequiredService<IRepository<HlgGame, Guid>>().GetQueryableAsync();
        var q = gameQ.Where(g => g.TenantId == CurrentTenant.Id && g.EndAt != null && g.EndAt < now);
        if (campaignGameIds.Count > 0) q = q.Where(g => campaignGameIds.Contains(g.Id));
        return await AsyncExecuter.ToListAsync(q.Select(g => g.Id));
    }

    /// <summary>Dựng các dòng kết quả passed-only từ sessions (dùng cho xuất báo cáo). KHÔNG side-effect (không snapshot/không reset).</summary>
    private async Task<List<HlgRankingResultExcelRow>> BuildRowsAsync(List<HlgGameSession> sessions)
    {
        if (sessions.Count == 0) return new List<HlgRankingResultExcelRow>();
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
        var eventScores = sessions.GroupBy(x => x.CustomerId)
            .ToDictionary(g => g.Key, g => g.Where(IsFinishedSessionPassed).Sum(y => y.Score));
        var ranks = eventScores.OrderByDescending(x => x.Value).ThenBy(x => x.Key)
            .Select((x, index) => new { x.Key, Rank = index + 1 }).ToDictionary(x => x.Key, x => x.Rank);
        return sessions.GroupBy(x => new { x.CustomerId, x.GameId })
            .Select(group =>
            {
                customerById.TryGetValue(group.Key.CustomerId, out var customer);
                gameById.TryGetValue(group.Key.GameId, out var game);
                var passed = group.Where(IsFinishedSessionPassed).ToList();
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
                    GameScore = passed.Sum(x => x.Score),
                    BestScore = passed.Count > 0 ? passed.Max(x => x.Score) : 0,
                    CorrectAnswerCount = passed.Sum(x => x.CorrectCount),
                    TotalQuestionCount = passed.Sum(x => x.TotalQuestions),
                    EventScore = eventScores[group.Key.CustomerId],
                    FirstPlayedAt = group.Min(x => x.FinishedAt)!.Value,
                    LastPlayedAt = group.Max(x => x.FinishedAt)!.Value
                };
            })
            .OrderBy(x => x.EventRank).ThenBy(x => x.PlayerName).ThenBy(x => x.GameName).ToList();
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
