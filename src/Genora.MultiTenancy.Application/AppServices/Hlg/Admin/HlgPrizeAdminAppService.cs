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
public class HlgPrizeAdminAppService : FeatureProtectedCrudAppService<HlgRankingPrize, HlgPrizeAdminDto, Guid, GetHlgAdminListInput, CreateHlgPrizeInput, UpdateHlgPrizeInput>, IHlgPrizeAdminAppService
{
    protected override string FeatureName => AppHlgFeatures.Management;
    protected override string TenantDefaultPermission => MultiTenancyPermissions.AppHlgRanking.Default;
    protected override string HostDefaultPermission => MultiTenancyPermissions.HostAppHlgRanking.Default;
    public HlgPrizeAdminAppService(IRepository<HlgRankingPrize, Guid> repository, ICurrentTenant tenant, IFeatureChecker features) : base(repository,tenant,features)
    {
        LocalizationResource = typeof(MultiTenancyResource);
        GetPolicyName = GetListPolicyName = TenantDefaultPermission;
        CreatePolicyName = TenantDefaultPermission + ".Create"; UpdatePolicyName = TenantDefaultPermission + ".Edit"; DeletePolicyName = TenantDefaultPermission + ".Delete";
    }
    private IRepository<T, Guid> Repo<T>() where T : class, Volo.Abp.Domain.Entities.IEntity<Guid> => LazyServiceProvider.LazyGetRequiredService<IRepository<T, Guid>>();
    private void Scope(Guid? id) => HlgContentValidation.Scope(id, CurrentTenant.Id);
    public override async Task<PagedResultDto<HlgPrizeAdminDto>> GetListAsync(GetHlgAdminListInput input)
    {
        await CheckGetListPolicyAsync();
        var query = (await Repository.GetQueryableAsync()).Where(x => x.TenantId == CurrentTenant.Id);
        if (!string.IsNullOrWhiteSpace(input.FilterText)) { var term = input.FilterText.Trim(); query = query.Where(x => x.Title.Contains(term)); }
        if (input.IsActive.HasValue) query = query.Where(x => x.IsActive == input.IsActive);
        if (input.ParentId.HasValue) query = query.Where(x => x.EventId == input.ParentId);
        var count = await AsyncExecuter.CountAsync(query);
        var rows = await AsyncExecuter.ToListAsync(query.OrderBy(x => x.DisplayOrder).ThenBy(x => x.Id).Skip(Math.Max(0,input.SkipCount)).Take(Math.Clamp(input.MaxResultCount,1,100)));
        return new(count, rows.Select(Map).ToList());
    }
    public override async Task<HlgPrizeAdminDto> GetAsync(Guid id) { await CheckGetPolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); return Map(entity); }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgPrizeAdminDto> CreateAsync(CreateHlgPrizeInput input)
    {
        await CheckCreatePolicyAsync(); await ValidateAsync(input,null);
        var entity = new HlgRankingPrize(GuidGenerator.Create(),CurrentTenant.Id); Apply(input,entity);
        await Repository.InsertAsync(entity,autoSave:true); return Map(entity);
    }
    [UnitOfWork(isTransactional: true)]
    public override async Task<HlgPrizeAdminDto> UpdateAsync(Guid id, UpdateHlgPrizeInput input)
    {
        await CheckUpdatePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId);
        await ValidateAsync(input,id); Apply(input,entity);
        await Repository.UpdateAsync(entity,autoSave:true); return Map(entity);
    }
    private async Task ValidateAsync(HlgPrizeInput input, Guid? id)
    { HlgContentValidation.Validate(input);
        var ev = await Repo<HlgRankingEvent>().GetAsync(input.EventId); Scope(ev.TenantId);
        var reward = await Repo<HlgReward>().GetAsync(input.RewardId); Scope(reward.TenantId);
        if (id.HasValue && await Repo<HlgRankingWinner>().AnyAsync(x => x.PrizeId == id)) {
            var old = await Repository.GetAsync(id.Value);
            if (old.EventId != input.EventId || old.RewardId != input.RewardId || old.IsActive != input.IsActive || input.Quantity < await Repo<HlgRankingWinner>().CountAsync(x => x.PrizeId == id && x.IsActive)) throw new UserFriendlyException(L["Hlg:PrizeHasWinners"]);
        }
 }
    public override async Task DeleteAsync(Guid id)
    { await CheckDeletePolicyAsync(); var entity = await Repository.GetAsync(id); Scope(entity.TenantId); if (await Repo<HlgRankingWinner>().AnyAsync(x => x.PrizeId == id)) throw new UserFriendlyException(L["Hlg:PrizeHasWinners"]); await Repository.DeleteAsync(entity); }
    private static void Apply(HlgPrizeInput input, HlgRankingPrize entity) { entity.EventId = input.EventId; entity.RewardId = input.RewardId; entity.Title = input.Title; entity.Quantity = input.Quantity; entity.DisplayOrder = input.DisplayOrder; entity.IsActive = input.IsActive; }
    private static HlgPrizeAdminDto Map(HlgRankingPrize entity) => new() { Id = entity.Id, EventId = entity.EventId, RewardId = entity.RewardId, Title = entity.Title, Quantity = entity.Quantity, DisplayOrder = entity.DisplayOrder, IsActive = entity.IsActive };
}
