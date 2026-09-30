using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Genora.MultiTenancy.AppDtos.Hlg;
using Genora.MultiTenancy.AppDtos.Hlg.Admin;
using Genora.MultiTenancy.DomainModels.AppHlg;
using Genora.MultiTenancy.DomainModels.AppCustomers;
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
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetEventEntriesAsync(entity.EventId, (await Repo<Customer>().GetAsync(entity.CustomerId)).PhoneNumber, 1);
        var row = entries.Single(x => x.UserId == entity.CustomerId); entity.Rank = row.Rank; entity.Score = row.Score;

        await Repository.InsertAsync(entity, autoSave: true); return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgWinnerAdminDto> UpdateAsync(Guid id, UpdateHlgWinnerInput input)
    {
        await CheckUpdatePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);
        await ValidateAsync(input, id); Apply(input, entity);
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetEventEntriesAsync(entity.EventId, (await Repo<Customer>().GetAsync(entity.CustomerId)).PhoneNumber, 1);
        var row = entries.Single(x => x.UserId == entity.CustomerId); entity.Rank = row.Rank; entity.Score = row.Score;

        await Repository.UpdateAsync(entity, autoSave: true); return Map(entity);
    }
    private async Task ValidateAsync(HlgWinnerInput input, Guid? id)
    {
        HlgContentValidation.Validate(input);
        var ev = await Repo<HlgRankingEvent>().GetAsync(input.EventId); Scope(ev.TenantId);
        var prize = await Repo<HlgRankingPrize>().GetAsync(input.PrizeId); Scope(prize.TenantId);
        var customer = await Repo<Customer>().GetAsync(input.CustomerId); Scope(customer.TenantId);
        if (prize.EventId != ev.Id || !prize.IsActive) throw new UserFriendlyException(L["Hlg:InvalidPrize"]);
        if (input.IsActive && Clock.Now <= ev.EndAt) throw new UserFriendlyException(L["Hlg:EventNotEnded"]);
        if (await Repository.AnyAsync(x => x.EventId == ev.Id && x.CustomerId == input.CustomerId && x.Id != id)) throw new UserFriendlyException(L["Hlg:WinnerAlreadyExists"]);
        if (input.IsActive && await Repository.CountAsync(x => x.PrizeId == prize.Id && x.IsActive && x.Id != id) >= prize.Quantity) throw new UserFriendlyException(L["Hlg:PrizeCapacityExceeded"]);
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetEventEntriesAsync(ev.Id, customer.PhoneNumber, 1);
        if (!entries.Any(x => x.UserId == input.CustomerId)) throw new UserFriendlyException(L["Hlg:WinnerHasNoScore"]);
        // Serialize concurrent publications against the same prize using ABP optimistic concurrency.
        //prize.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        await Repo<HlgRankingPrize>().UpdateAsync(prize, autoSave: true);
    }
    public override async Task DeleteAsync(Guid id)
    { await CheckDeletePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); await Repository.DeleteAsync(entity); }

    public async Task<IRemoteStreamContent> DownloadImportTemplateAsync()
    {
        await CheckGetListPolicyAsync();
        var events = await Repo<HlgRankingEvent>().GetListAsync(x => x.TenantId == CurrentTenant.Id);
        var eventIds = events.Select(x => x.Id).ToList();
        var prizes = await Repo<HlgRankingPrize>().GetListAsync(x => x.TenantId == CurrentTenant.Id && eventIds.Contains(x.EventId) && x.IsActive);
        return LazyServiceProvider.LazyGetRequiredService<HlgWinnerExcelTemplateGenerator>().Generate(
            events.OrderByDescending(x => x.StartAt).ThenBy(x => x.Title),
            prizes.OrderBy(x => x.EventId).ThenBy(x => x.DisplayOrder).ThenBy(x => x.Title));
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
                if (!TryParseLookupId(row.PrizeId, out var prizeId))
                    throw new UserFriendlyException(L["Hlg:WinnerImportPrizeInvalid"]);
                if (string.IsNullOrWhiteSpace(row.CustomerPhone))
                    throw new UserFriendlyException(L["Hlg:WinnerImportPhoneRequired"]);
                if (!TryParseBoolean(row.IsActive, false, out var isActive))
                    throw new UserFriendlyException(L["Hlg:WinnerImportIsActiveInvalid"]);

                var rankingEvent = await Repo<HlgRankingEvent>().FirstOrDefaultAsync(x => x.Id == eventId && x.TenantId == CurrentTenant.Id);
                if (rankingEvent == null) throw new UserFriendlyException(L["Hlg:WinnerImportEventInvalid"]);
                var prize = await Repo<HlgRankingPrize>().FirstOrDefaultAsync(x => x.Id == prizeId && x.TenantId == CurrentTenant.Id && x.EventId == eventId);
                if (prize == null) throw new UserFriendlyException(L["Hlg:WinnerImportPrizeInvalid"]);
                var phone = row.CustomerPhone.Trim();
                var customer = await Repo<Customer>().FirstOrDefaultAsync(x => x.TenantId == CurrentTenant.Id && x.PhoneNumber == phone);
                if (customer == null) throw new UserFriendlyException(L["Hlg:WinnerImportCustomerNotFound", phone]);

                await CreateWinnerAsync(new HlgWinnerInput
                {
                    EventId = eventId,
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

    private static void Apply(HlgWinnerInput input, HlgRankingWinner entity) { entity.EventId = input.EventId; entity.PrizeId = input.PrizeId; entity.CustomerId = input.CustomerId; entity.IsActive = input.IsActive; }
    private static HlgWinnerAdminDto Map(HlgRankingWinner entity) => new() { Id = entity.Id, EventId = entity.EventId, PrizeId = entity.PrizeId, CustomerId = entity.CustomerId, IsActive = entity.IsActive, Rank = entity.Rank, Score = entity.Score };
}
