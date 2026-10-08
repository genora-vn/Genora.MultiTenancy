using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
using Genora.MultiTenancy.Enums.Hlg;
using Genora.MultiTenancy.Features.AppHlgFeatures;
using Genora.MultiTenancy.Localization;
using Genora.MultiTenancy.Permissions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Content;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
using Volo.Abp.Validation;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;
[Authorize]
public class HlgWinnerAdminAppService : FeatureProtectedCrudAppService<HlgRankingWinner, HlgWinnerAdminDto, Guid, GetHlgAdminListInput, CreateHlgWinnerInput, UpdateHlgWinnerInput>, IHlgWinnerAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgRanking.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgRanking.Default;
    public HlgWinnerAdminAppService(IRepository<HlgRankingWinner, Guid> repository, ICurrentTenant tenant, IFeatureChecker features) : base(repository, tenant, features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        GetPolicyName = GetListPolicyName = TenantDefaultPermission;
        CreatePolicyName = TenantDefaultPermission + ".Create"; UpdatePolicyName = TenantDefaultPermission + ".Edit"; DeletePolicyName = TenantDefaultPermission + ".Delete";
    }
    private IRepository<T, Guid> Repo<T>() where T : class, Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();
    private void Scope(Guid? id) => HlgContentValidation.Scope(id, CurrentTenant.Id);
    public override async Task<PagedResultDto<HlgWinnerAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = (await Repository.GetQueryableAsync()).Where(x => x.TenantId == CurrentTenant.Id);

        if (!string.IsNullOrWhiteSpace(input.FilterText))
        {
            var term = input.FilterText.Trim(); var players = await Repo<Customer>().GetQueryableAsync();
            query = query.Where(x => players.Any(c => c.Id == x.CustomerId && c.FullName.Contains(term)));
        }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive);
        if (input.ParentId.HasValue) query = query.Where(x => x.EventId == input.ParentId);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Rank).ThenBy(x => x.Id).Skip(Math.Max(0, input.SkipCount)).Take(Math.Clamp(input.MaxResultCount, 1, 100)));
        var ids = rows.Select(x => x.CustomerId).ToList();
        var customers = await AsyncExecuter.ToListAsync((await Repo<Customer>().GetQueryableAsync()).Where(x => ids.Contains(x.Id)));
        var dtos = rows.Select(Map).ToList(); foreach (var dto in dtos) dto.CustomerName = customers.FirstOrDefault(x => x.Id == dto.CustomerId)?.FullName ?? "";
        return new(count, dtos);
    }
    public override async Task<HlgWinnerAdminDto> GetAsync(Guid id) { await CheckGetPolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); return Map(entity); }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgWinnerAdminDto> CreateAsync(CreateHlgWinnerInput input)
    {
        await CheckCreatePolicyAsync();
        return await CreateWinnerAsync(input);
    }
    private async Task<HlgWinnerAdminDto> CreateWinnerAsync(HlgWinnerInput input)
    {
        await ValidateAsync(input, null);
        var entity = new HlgRankingWinner(GuidGenerator.Create(), CurrentTenant.Id); Apply(input, entity);
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetGameEntriesAsync(entity.EventId, entity.GameId!.Value, (await Repo<Customer>().GetAsync(entity.CustomerId)).PhoneNumber, 1);
        var row = entries.Single(x => x.UserId == entity.CustomerId); entity.Rank = row.Rank; entity.Score = row.Score;

        await Repository.InsertAsync(entity, autoSave: true);
        if (entity.IsActive)
        {
            await AdjustRewardStockAsync(entity.PrizeId, -1); // trao (công bố) => trừ kho quà
            await SyncRewardHistoryAsync(entity); // ghi lịch sử nhận quà để API reward-history hiển thị
        }
        return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgWinnerAdminDto> UpdateAsync(Guid id, UpdateHlgWinnerInput input)
    {
        await CheckUpdatePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);
        var wasActive = entity.IsActive; var oldPrizeId = entity.PrizeId; // trạng thái kho trước khi sửa
        await ValidateAsync(input, id); Apply(input, entity);
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetGameEntriesAsync(entity.EventId, entity.GameId!.Value, (await Repo<Customer>().GetAsync(entity.CustomerId)).PhoneNumber, 1);
        var row = entries.Single(x => x.UserId == entity.CustomerId); entity.Rank = row.Rank; entity.Score = row.Score;

        await Repository.UpdateAsync(entity, autoSave: true);
        if (wasActive) await AdjustRewardStockAsync(oldPrizeId, +1);         // hoàn kho giải cũ nếu trước đó đang trao
        if (entity.IsActive) await AdjustRewardStockAsync(entity.PrizeId, -1); // trừ kho giải mới nếu đang trao
        if (wasActive && !entity.IsActive) await RemoveRewardHistoryAsync(entity.Id); // bỏ công bố => xóa lịch sử nhận quà
        else if (entity.IsActive) await SyncRewardHistoryAsync(entity); // công bố mới hoặc đổi giải => đồng bộ lịch sử
        return Map(entity);
    }
    private async Task ValidateAsync(HlgWinnerInput input, Guid? id)
    {
        HlgContentValidation.Validate(input);
        var ev = await Repo<HlgRankingEvent>().GetAsync(input.EventId); Scope(ev.TenantId);
        var prize = await Repo<HlgRankingPrize>().GetAsync(input.PrizeId); Scope(prize.TenantId);
        var customer = await Repo<Customer>().GetAsync(input.CustomerId); Scope(customer.TenantId);
        var game = await Repo<HlgGame>().GetAsync(input.GameId); Scope(game.TenantId);
        if (prize.EventId != ev.Id || !prize.IsActive) throw new UserFriendlyException(L["Hlg:InvalidPrize"]);
        // Game phải thuộc sự kiện (event single-game: ev.GameId; event nhiều chặng: bảng map HlgRankingEventGame).
        if (ev.GameId != input.GameId && !await Repo<HlgRankingEventGame>().AnyAsync(x => x.EventId == ev.Id && x.GameId == input.GameId))
            throw new UserFriendlyException(L["Hlg:GameNotInEvent"]);
        // Chỉ được TRAO (công bố) sau khi GAME đã kết thúc (EndAt < nay). EndAt null = chưa cấu hình kết thúc.
        if (input.IsActive && (game.EndAt == null || Clock.Now <= game.EndAt)) throw new UserFriendlyException(L["Hlg:GameNotEnded"]);
        // Trùng: 1 người chỉ 1 winner cho CÙNG 1 game (được phép trúng ở các game khác trong sự kiện).
        if (await Repository.AnyAsync(x => x.EventId == ev.Id && x.GameId == input.GameId && x.CustomerId == input.CustomerId && x.Id != id)) throw new UserFriendlyException(L["Hlg:WinnerAlreadyExists"]);
        if (input.IsActive && await Repository.CountAsync(x => x.PrizeId == prize.Id && x.IsActive && x.Id != id) >= prize.Quantity) throw new UserFriendlyException(L["Hlg:PrizeCapacityExceeded"]);
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetGameEntriesAsync(ev.Id, input.GameId, customer.PhoneNumber, 1);
        if (!entries.Any(x => x.UserId == input.CustomerId)) throw new UserFriendlyException(L["Hlg:WinnerHasNoScore"]);
        // Serialize concurrent publications against the same prize using ABP optimistic concurrency.
        //prize.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        await Repo<HlgRankingPrize>().UpdateAsync(prize, autoSave: true);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task DeleteAsync(Guid id)
    {
        await CheckDeletePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);
        await Repository.DeleteAsync(entity);
        if (entity.IsActive) await AdjustRewardStockAsync(entity.PrizeId, +1); // hủy winner đang trao => hoàn kho quà
    }

    /// <summary>
    /// Điều chỉnh kho phần quà của giải: delta &lt; 0 = TRỪ kho (khi trao/công bố), delta &gt; 0 = HOÀN kho (khi hủy/bỏ công bố).
    /// StockQuantity null = không giới hạn (bỏ qua). Nếu trừ quá kho sẽ chặn và báo lỗi để tăng cấu hình tồn kho.
    /// Chạy trong transaction của Create/Update/Delete; cập nhật dùng optimistic concurrency của HlgReward để chống âm kho khi trao đồng thời.
    /// </summary>
    private async Task AdjustRewardStockAsync(Guid prizeId, int delta)
    {
        var prize = await Repo<HlgRankingPrize>().GetAsync(prizeId);
        if (prize.RewardId == Guid.Empty) return;
        var reward = await Repo<HlgReward>().FirstOrDefaultAsync(x => x.Id == prize.RewardId && x.TenantId == CurrentTenant.Id);
        if (reward == null || !reward.StockQuantity.HasValue) return; // null = không giới hạn
        var next = reward.StockQuantity.Value + delta;
        if (next < 0) throw new UserFriendlyException(L["Hlg:RewardOutOfStock"]);
        reward.StockQuantity = next;
        await Repo<HlgReward>().UpdateAsync(reward, autoSave: true);
    }

    /// <summary>
    /// Đồng bộ "Lịch sử nhận quà" (HlgRewardHistory) khi 1 winner được công bố (IsActive=true), để API
    /// profile/reward-history (mini app) hiển thị đúng. Idempotent: nếu đã có record cho winner này thì cập nhật,
    /// chưa có thì tạo mới. RewardId rỗng (giải không gắn quà) => bỏ qua, không tạo lịch sử.
    /// </summary>
    private async Task SyncRewardHistoryAsync(HlgRankingWinner winner)
    {
        var prize = await Repo<HlgRankingPrize>().GetAsync(winner.PrizeId);
        if (prize.RewardId == Guid.Empty) return;
        var reward = await Repo<HlgReward>().FirstOrDefaultAsync(x => x.Id == prize.RewardId && x.TenantId == CurrentTenant.Id);
        if (reward == null) return;

        var historyRepo = Repo<HlgRewardHistory>();
        var existing = await historyRepo.FirstOrDefaultAsync(x => x.CustomerId == winner.CustomerId && x.RewardId == reward.Id && x.TenantId == CurrentTenant.Id && x.WinnerId == winner.Id);
        var status = reward.Type == HlgRewardType.Voucher ? HlgRewardHistoryStatus.Done : HlgRewardHistoryStatus.Pending;
        if (existing != null)
        {
            existing.RewardId = reward.Id; existing.RewardName = reward.Name; existing.RewardType = reward.Type;
            await historyRepo.UpdateAsync(existing, autoSave: true);
            return;
        }
        var history = new HlgRewardHistory(GuidGenerator.Create(), winner.CustomerId, reward.Id, reward.Name, CurrentTenant.Id)
        {
            PointDelta = 0, // trao giải thưởng (không trừ điểm, khác với tự đổi quà bằng điểm)
            RewardType = reward.Type,
            Status = status,
            VoucherCode = reward.Type == HlgRewardType.Voucher ? reward.VoucherCode : null,
            WinnerId = winner.Id
        };
        await historyRepo.InsertAsync(history, autoSave: true);
    }

    /// <summary>Xóa "Lịch sử nhận quà" tương ứng khi winner bị hủy/bỏ công bố.</summary>
    private async Task RemoveRewardHistoryAsync(Guid winnerId)
    {
        var historyRepo = Repo<HlgRewardHistory>();
        var existing = await historyRepo.GetListAsync(x => x.WinnerId == winnerId && x.TenantId == CurrentTenant.Id);
        foreach (var h in existing) await historyRepo.DeleteAsync(h);
    }

    /// <summary>Trao giải hàng loạt cho 1 game + 1 giải. All-or-nothing: 1 người lỗi => rollback cả lô (giữ kho quà nhất quán).</summary>
    [UnitOfWork(isTransactional: true)]
    public async Task<int> CreateManyAsync(CreateHlgWinnersBatchInput input)
    {
        await CheckCreatePolicyAsync();
        var customerIds = (input.CustomerIds ?? new()).Distinct().ToList();
        if (customerIds.Count == 0) throw new UserFriendlyException(L["Hlg:WinnerImportNoData"]);
        var count = 0;
        foreach (var customerId in customerIds)
        {
            await CreateWinnerAsync(new HlgWinnerInput { EventId = input.EventId, GameId = input.GameId, PrizeId = input.PrizeId, CustomerId = customerId, IsActive = input.IsActive });
            count++;
        }
        return count;
    }

    /// <summary>
    /// Danh sách game đã kết thúc (EndAt &lt; nay) để chọn trao giải.
    /// Ưu tiên bảng map nhiều chặng (HlgRankingEventGame); nếu rỗng dùng GameId đơn (legacy);
    /// nếu sự kiện không giới hạn game nào (không map + không GameId) thì KHÔNG giới hạn — trả về toàn bộ
    /// game đã kết thúc của tenant (khớp logic GetEndedCampaignGamesAsync bên Ranking export).
    /// </summary>
    public async Task<List<HlgEndedGameLookupDto>> GetEndedEventGamesAsync(Guid eventId)
    {
        await CheckGetListPolicyAsync();
        var ev = await Repo<HlgRankingEvent>().GetAsync(eventId); Scope(ev.TenantId);
        var mapped = await AsyncExecuter.ToListAsync((await Repo<HlgRankingEventGame>().GetQueryableAsync()).Where(x => x.EventId == eventId).Select(x => x.GameId));
        var gameIds = mapped.Count > 0 ? mapped.Distinct().ToList() : (ev.GameId.HasValue ? new List<Guid> { ev.GameId.Value } : new List<Guid>());
        var gameQ = await Repo<HlgGame>().GetQueryableAsync();
        // Phòng mapping/GameId legacy bị "treo" (trỏ tới game đã xóa, ví dụ do xóa dữ liệu test):
        // nếu KHÔNG còn game nào trong danh sách map thực sự tồn tại (chưa bị xóa), coi như sự kiện
        // "không giới hạn game" (giống trường hợp chưa cấu hình map) để tránh dropdown trống oan.
        if (gameIds.Count > 0)
        {
            var existingCount = await AsyncExecuter.CountAsync(gameQ.Where(g => g.TenantId == CurrentTenant.Id && gameIds.Contains(g.Id)));
            if (existingCount == 0) gameIds = new List<Guid>();
        }
        var now = Clock.Now;
        var q = gameQ.Where(g => g.TenantId == CurrentTenant.Id && g.EndAt != null && g.EndAt < now);
        if (gameIds.Count > 0) q = q.Where(g => gameIds.Contains(g.Id));
        var games = await AsyncExecuter.ToListAsync(q);
        return games.OrderByDescending(g => g.EndAt).Select(g => new HlgEndedGameLookupDto { Id = g.Id, Name = g.Name, EndAt = g.EndAt }).ToList();
    }

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        await CheckGetListPolicyAsync();
        var events = await Repo<HlgRankingEvent>().GetListAsync(x => x.TenantId == CurrentTenant.Id);
        var eventIds = events.Select(x => x.Id).ToList();
        var prizes = await Repo<HlgRankingPrize>().GetListAsync(x => x.TenantId == CurrentTenant.Id && eventIds.Contains(x.EventId) && x.IsActive);
        var now = Clock.Now;
        var games = await Repo<HlgGame>().GetListAsync(x => x.TenantId == CurrentTenant.Id && x.EndAt != null && x.EndAt < now);
        var rewardIds = prizes.Select(x => x.RewardId).Distinct().ToList();
        var rewards = await Repo<HlgReward>().GetListAsync(x => x.TenantId == CurrentTenant.Id && rewardIds.Contains(x.Id));
        return LazyServiceProvider.LazyGetRequiredService<HlgWinnerExcelTemplateGenerator>().Generate(
            events.OrderByDescending(x => x.StartAt).ThenBy(x => x.Title),
            games.OrderByDescending(x => x.EndAt).ThenBy(x => x.Name),
            prizes.OrderBy(x => x.EventId).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Title),
            rewards);
    }

    [DisableValidation]
    [UnitOfWork(isTransactional: true)]
    public async Task<int> ImportExcelAsync(ImportHlgWinnerExcelInput input)
    {
        await CheckCreatePolicyAsync();
        if (input.File == null) throw new UserFriendlyException(L["Hlg:ImportFileRequired"]);
        if (!(input.File.FileName ?? "").EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
            throw new UserFriendlyException(L["Hlg:ImportFileTypeInvalid"]);
        if ((input.File.ContentLength ?? 0) > 10 * 1024 * 1024)
            throw new UserFriendlyException(L["Hlg:ImportFileTooLarge"]);

        List<HlgWinnerExcelRow> rows;
        try
        {
            using var stream = input.File.GetStream();
            rows = LazyServiceProvider.LazyGetRequiredService<HlgWinnerExcelImporter>().Read(stream);
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Cannot read HLG winner import workbook");
            throw new UserFriendlyException(L["Hlg:ImportFileInvalid"]);
        }

        if (rows.Count == 0) throw new UserFriendlyException(L["Hlg:WinnerImportNoData"]);

        var success = 0;
        foreach (var row in rows)
        {
            try
            {
                if (!TryParseLookupId(row.EventId, out var eventId))
                    throw new UserFriendlyException(L["Hlg:WinnerImportEventInvalid"]);
                if (!TryParseLookupId(row.GameId, out var gameId))
                    throw new UserFriendlyException(L["Hlg:WinnerImportGameInvalid"]);
                if (!TryParseLookupId(row.PrizeId, out var prizeId))
                    throw new UserFriendlyException(L["Hlg:WinnerImportPrizeInvalid"]);
                if (string.IsNullOrWhiteSpace(row.CustomerPhone))
                    throw new UserFriendlyException(L["Hlg:WinnerImportPhoneRequired"]);
                if (!TryParseBoolean(row.IsActive, false, out var isActive))
                    throw new UserFriendlyException(L["Hlg:WinnerImportIsActiveInvalid"]);

                var rankingEvent = await Repo<HlgRankingEvent>().FirstOrDefaultAsync(x => x.Id == eventId && x.TenantId == CurrentTenant.Id);
                if (rankingEvent == null) throw new UserFriendlyException(L["Hlg:WinnerImportEventInvalid"]);
                var game = await Repo<HlgGame>().FirstOrDefaultAsync(x => x.Id == gameId && x.TenantId == CurrentTenant.Id);
                if (game == null) throw new UserFriendlyException(L["Hlg:WinnerImportGameInvalid"]);
                var prize = await Repo<HlgRankingPrize>().FirstOrDefaultAsync(x => x.Id == prizeId && x.TenantId == CurrentTenant.Id && x.EventId == eventId);
                if (prize == null) throw new UserFriendlyException(L["Hlg:WinnerImportPrizeInvalid"]);
                var phone = row.CustomerPhone.Trim();
                var customer = await Repo<Customer>().FirstOrDefaultAsync(x => x.TenantId == CurrentTenant.Id && x.PhoneNumber == phone);
                if (customer == null) throw new UserFriendlyException(L["Hlg:WinnerImportCustomerNotFound", phone]);

                await CreateWinnerAsync(new HlgWinnerInput
                {
                    EventId = eventId,
                    GameId = gameId,
                    PrizeId = prizeId,
                    CustomerId = customer.Id,
                    IsActive = isActive
                });
                success++;
            }
            catch (UserFriendlyException ex)
            {
                throw new UserFriendlyException(L["Hlg:WinnerImportRowInvalid", row.RowNumber, ex.Message]);
            }
            catch (System.ComponentModel.DataAnnotations.ValidationException ex)
            {
                throw new UserFriendlyException(L["Hlg:WinnerImportRowInvalid", row.RowNumber, ex.Message]);
            }
        }

        return success;
    }

    private static bool TryParseLookupId(string value, out Guid result)
    {
        var normalized = value.Trim();
        if (Guid.TryParse(normalized, out result)) return true;
        var separator = normalized.LastIndexOf('|');
        return separator >= 0 && Guid.TryParse(normalized[(separator + 1)..].Trim(), out result);
    }

    private static bool TryParseBoolean(string value, bool defaultValue, out bool result)
    {
        if (string.IsNullOrWhiteSpace(value)) { result = defaultValue; return true; }
        var normalized = value.Trim();
        if (bool.TryParse(normalized, out result)) return true;
        if (normalized == "1" || normalized.Equals("yes", StringComparison.OrdinalIgnoreCase) || normalized.Equals("có", StringComparison.OrdinalIgnoreCase)) { result = true; return true; }
        if (normalized == "0" || normalized.Equals("no", StringComparison.OrdinalIgnoreCase) || normalized.Equals("không", StringComparison.OrdinalIgnoreCase)) { result = false; return true; }
        return false;
    }

    private static void Apply(HlgWinnerInput input, HlgRankingWinner entity) { entity.EventId = input.EventId; entity.GameId = input.GameId; entity.PrizeId = input.PrizeId; entity.CustomerId = input.CustomerId; entity.IsActive = input.IsActive; }
    private static HlgWinnerAdminDto Map(HlgRankingWinner entity) => new() { Id = entity.Id, EventId = entity.EventId, GameId = entity.GameId ?? Guid.Empty, PrizeId = entity.PrizeId, CustomerId = entity.CustomerId, IsActive = entity.IsActive, Rank = entity.Rank, Score = entity.Score };
}
