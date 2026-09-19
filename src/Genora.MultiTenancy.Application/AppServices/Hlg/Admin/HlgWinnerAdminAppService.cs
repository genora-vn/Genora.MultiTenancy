using System;
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
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Features;
using Volo.Abp.MultiTenancy;
using Volo.Abp.Uow;
namespace Genora.MultiTenancy.AppServices.Hlg.Admin;
[Authorize]
public class HlgWinnerAdminAppService : FeatureProtectedCrudAppService<HlgRankingWinner, HlgWinnerAdminDto, Guid, GetHlgAdminListInput, CreateHlgWinnerInput, UpdateHlgWinnerInput>, IHlgWinnerAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgRanking.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgRanking.Default;
    public HlgWinnerAdminAppService(IRepository<HlgRankingWinner, Guid> repository, ICurrentTenant tenant, IFeatureChecker features) : base(repository,tenant,features)
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

        if (!string.IsNullOrWhiteSpace(input.FilterText)) {
            var term=input.FilterText.Trim();var players=await Repo<Customer>().GetQueryableAsync();
            query=query.Where(x=>players.Any(c=>c.Id==x.CustomerId && c.FullName.Contains(term)));
        }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive);
        if (input.ParentId.HasValue) query = query.Where(x => x.EventId == input.ParentId);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.Rank).ThenBy(x => x.Id).Skip(Math.Max(0,input.SkipCount)).Take(Math.Clamp(input.MaxResultCount,1,100)));
        var ids = rows.Select(x=>x.CustomerId).ToList();
        var customers = await AsyncExecuter.ToListAsync((await Repo<Customer>().GetQueryableAsync()).Where(x=>ids.Contains(x.Id)));
        var dtos=rows.Select(Map).ToList(); foreach(var dto in dtos) dto.CustomerName=customers.FirstOrDefault(x=>x.Id==dto.CustomerId)?.FullName ?? "";
        return new(count, dtos);
    }
    public override async Task<HlgWinnerAdminDto> GetAsync(Guid id) { await CheckGetPolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); return Map(entity); }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgWinnerAdminDto> CreateAsync(CreateHlgWinnerInput input)
    {
        await CheckCreatePolicyAsync(); await ValidateAsync(input,null);
        var entity = new HlgRankingWinner(GuidGenerator.Create(),CurrentTenant.Id); Apply(input,entity);
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetEventEntriesAsync(entity.EventId, (await Repo<Customer>().GetAsync(entity.CustomerId)).PhoneNumber, 1);
        var row = entries.Single(x => x.UserId == entity.CustomerId); entity.Rank = row.Rank; entity.Score = row.Score;

        await Repository.InsertAsync(entity,autoSave:true); return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgWinnerAdminDto> UpdateAsync(Guid id, UpdateHlgWinnerInput input)
    {
        await CheckUpdatePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);
        await ValidateAsync(input,id); Apply(input,entity);
        var entries = await LazyServiceProvider.LazyGetRequiredService<IHlgRankingAppService>().GetEventEntriesAsync(entity.EventId, (await Repo<Customer>().GetAsync(entity.CustomerId)).PhoneNumber, 1);
        var row = entries.Single(x => x.UserId == entity.CustomerId); entity.Rank = row.Rank; entity.Score = row.Score;

        await Repository.UpdateAsync(entity,autoSave:true); return Map(entity);
    }
    private async Task ValidateAsync(HlgWinnerInput input, Guid? id)
    { HlgContentValidation.Validate(input);
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
        prize.ConcurrencyStamp = Guid.NewGuid().ToString("N");
        await Repo<HlgRankingPrize>().UpdateAsync(prize, autoSave: true);
 }
    public override async Task DeleteAsync(Guid id)
    { await CheckDeletePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);  await Repository.DeleteAsync(entity); }
    private static void Apply(HlgWinnerInput input, HlgRankingWinner entity) { entity.EventId = input.EventId; entity.PrizeId = input.PrizeId; entity.CustomerId = input.CustomerId; entity.IsActive = input.IsActive; }
    private static HlgWinnerAdminDto Map(HlgRankingWinner entity) => new() { Id = entity.Id, EventId = entity.EventId, PrizeId = entity.PrizeId, CustomerId = entity.CustomerId, IsActive = entity.IsActive, Rank = entity.Rank, Score = entity.Score };
}
